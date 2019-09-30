// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Microsoft.Build.Utilities;
    using Mono.Cecil;
    using Mono.Cecil.Rocks;

    internal class Program
    {
        internal static void Main(SuppressibleLoggingHelper? log, string referenceAssembly, string annotatedReferenceAssembly, string outputAssembly)
        {
            var assemblyResolver = new DefaultAssemblyResolver();
            assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(referenceAssembly));
            using var assemblyDefinition = AssemblyDefinition.ReadAssembly(referenceAssembly, new ReaderParameters(ReadingMode.Immediate) { AssemblyResolver = assemblyResolver });

            foreach (var module in assemblyDefinition.Modules)
            {
                if (!module.Attributes.HasFlag(ModuleAttributes.ILOnly))
                {
                    log?.LogWarning("RA1000", "Skipping mixed-mode implementation assembly '{0}'", assemblyDefinition.Name);
                    return;
                }
            }

            var annotatedAssemblyResolver = new DefaultAssemblyResolver();
            annotatedAssemblyResolver.AddSearchDirectory(Path.GetDirectoryName(annotatedReferenceAssembly));
            using var annotatedAssemblyDefinition = AssemblyDefinition.ReadAssembly(annotatedReferenceAssembly, new ReaderParameters(ReadingMode.Immediate) { AssemblyResolver = annotatedAssemblyResolver });

            var wellKnownTypes = new WellKnownTypes(assemblyDefinition, DefineReferenceAssemblyAttribute);

            // Define embedded types used by the compiler
            var embeddedAttribute = DefineEmbeddedAttribute(assemblyDefinition, wellKnownTypes);
            var nullableAttribute = DefineNullableAttribute(assemblyDefinition, embeddedAttribute, wellKnownTypes);
            var nullableContextAttribute = DefineNullableContextAttribute(assemblyDefinition, embeddedAttribute, wellKnownTypes);
            var nullablePublicOnlyAttribute = DefineNullablePublicOnlyAttribute(assemblyDefinition, embeddedAttribute, wellKnownTypes);

            var attributeFactory = new CustomAttributeFactory(wellKnownTypes, embeddedAttribute, nullableAttribute, nullableContextAttribute, nullablePublicOnlyAttribute);

            // Define attributes for annotating nullable types
            var allowNullAttribute = DefineAllowNullAttribute(assemblyDefinition, wellKnownTypes, attributeFactory);
            var disallowNullAttribute = DefineDisallowNullAttribute(assemblyDefinition, wellKnownTypes, attributeFactory);
            var doesNotReturnAttribute = DefineDoesNotReturnAttribute(assemblyDefinition, wellKnownTypes, attributeFactory);
            var doesNotReturnIfAttribute = DefineDoesNotReturnIfAttribute(assemblyDefinition, wellKnownTypes, attributeFactory);
            var maybeNullAttribute = DefineMaybeNullAttribute(assemblyDefinition, wellKnownTypes, attributeFactory);
            var maybeNullWhenAttribute = DefineMaybeNullWhenAttribute(assemblyDefinition, wellKnownTypes, attributeFactory);
            var notNullAttribute = DefineNotNullAttribute(assemblyDefinition, wellKnownTypes, attributeFactory);
            var notNullIfNotNullAttribute = DefineNotNullIfNotNullAttribute(assemblyDefinition, wellKnownTypes, attributeFactory);
            var notNullWhenAttribute = DefineNotNullWhenAttribute(assemblyDefinition, wellKnownTypes, attributeFactory);

            // Ensure the assembly is marked with ReferenceAssemblyAttribute
            EnsureReferenceAssemblyAttribute(assemblyDefinition, attributeFactory);

            var attributesOfInterest = new Dictionary<string, TypeDefinition>();
            attributesOfInterest.Add(nullableAttribute.FullName, nullableAttribute);
            attributesOfInterest.Add(nullableContextAttribute.FullName, nullableContextAttribute);
            attributesOfInterest.Add(nullablePublicOnlyAttribute.FullName, nullablePublicOnlyAttribute);
            attributesOfInterest.Add(allowNullAttribute.FullName, allowNullAttribute);
            attributesOfInterest.Add(disallowNullAttribute.FullName, disallowNullAttribute);
            attributesOfInterest.Add(doesNotReturnAttribute.FullName, doesNotReturnAttribute);
            attributesOfInterest.Add(doesNotReturnIfAttribute.FullName, doesNotReturnIfAttribute);
            attributesOfInterest.Add(maybeNullAttribute.FullName, maybeNullAttribute);
            attributesOfInterest.Add(maybeNullWhenAttribute.FullName, maybeNullWhenAttribute);
            attributesOfInterest.Add(notNullAttribute.FullName, notNullAttribute);
            attributesOfInterest.Add(notNullIfNotNullAttribute.FullName, notNullIfNotNullAttribute);
            attributesOfInterest.Add(notNullWhenAttribute.FullName, notNullWhenAttribute);

            AnnotateAssembly(log?.Helper, assemblyDefinition, annotatedAssemblyDefinition, attributesOfInterest);

            assemblyDefinition.Write(outputAssembly);
        }

        private static void AnnotateAssembly(TaskLoggingHelper? log, AssemblyDefinition assemblyDefinition, AssemblyDefinition annotatedAssemblyDefinition, Dictionary<string, TypeDefinition> attributesOfInterest)
        {
            Annotate(assemblyDefinition, annotatedAssemblyDefinition, attributesOfInterest);
            if (assemblyDefinition.Modules.Count != 1)
                throw new NotSupportedException();

            AnnotateModule(log, assemblyDefinition.MainModule, annotatedAssemblyDefinition.MainModule, attributesOfInterest);
        }

        private static void AnnotateModule(TaskLoggingHelper? log, ModuleDefinition moduleDefinition, ModuleDefinition annotatedModuleDefinition, Dictionary<string, TypeDefinition> attributesOfInterest)
        {
            Annotate(moduleDefinition, annotatedModuleDefinition, attributesOfInterest);
            foreach (var type in moduleDefinition.GetAllTypes())
            {
                AnnotateType(log, type, annotatedModuleDefinition, attributesOfInterest);
            }
        }

        private static void AnnotateType(TaskLoggingHelper? log, TypeDefinition typeDefinition, ModuleDefinition annotatedModuleDefinition, Dictionary<string, TypeDefinition> attributesOfInterest)
        {
            if (attributesOfInterest.ContainsKey(typeDefinition.FullName))
                return;

            var annotatedTypeDefinition = FindMatchingType(log, typeDefinition, annotatedModuleDefinition);
            if (annotatedTypeDefinition is null)
            {
                return;
            }

            Annotate(typeDefinition, annotatedTypeDefinition, attributesOfInterest);
            for (int i = 0; i < typeDefinition.Interfaces.Count; i++)
            {
                for (int j = 0; j < annotatedTypeDefinition.Interfaces.Count; j++)
                {
                    if (EquivalenceComparers.TypeReference.Equals(typeDefinition.Interfaces[i].InterfaceType, annotatedTypeDefinition.Interfaces[j].InterfaceType))
                    {
                        Annotate(typeDefinition.Interfaces[i], annotatedTypeDefinition.Interfaces[j], attributesOfInterest);
                    }
                }
            }

            for (int i = 0; i < typeDefinition.GenericParameters.Count; i++)
            {
                Annotate(typeDefinition.GenericParameters[i], annotatedTypeDefinition.GenericParameters[i], attributesOfInterest);
            }

            foreach (var method in typeDefinition.Methods)
            {
                AnnotateMethod(log, method, annotatedTypeDefinition, attributesOfInterest);
            }

            foreach (var property in typeDefinition.Properties)
            {
                AnnotateProperty(property, annotatedTypeDefinition, attributesOfInterest);
            }

            foreach (var field in typeDefinition.Fields)
            {
                AnnotateField(field, annotatedTypeDefinition, attributesOfInterest);
            }
        }

        private static void AnnotateMethod(TaskLoggingHelper? log, MethodDefinition methodDefinition, TypeDefinition annotatedTypeDefinition, Dictionary<string, TypeDefinition> attributesOfInterest)
        {
            var annotatedMethodDefinition = FindMatchingMethod(log, methodDefinition, annotatedTypeDefinition);
            if (annotatedMethodDefinition is null)
                return;

            Annotate(methodDefinition, annotatedMethodDefinition, attributesOfInterest);
            Annotate(methodDefinition.MethodReturnType, annotatedMethodDefinition.MethodReturnType, attributesOfInterest);
            for (int i = 0; i < methodDefinition.Parameters.Count; i++)
            {
                Annotate(methodDefinition.Parameters[i], annotatedMethodDefinition.Parameters[i], attributesOfInterest);
            }

            for (int i = 0; i < methodDefinition.GenericParameters.Count; i++)
            {
                Annotate(methodDefinition.GenericParameters[i], annotatedMethodDefinition.GenericParameters[i], attributesOfInterest);
            }
        }

        private static void AnnotateProperty(PropertyDefinition propertyDefinition, TypeDefinition annotatedTypeDefinition, Dictionary<string, TypeDefinition> attributesOfInterest)
        {
            var annotatedPropertyDefinition = FindMatchingProperty(propertyDefinition, annotatedTypeDefinition);
            if (annotatedPropertyDefinition is null)
                return;

            Annotate(propertyDefinition, annotatedPropertyDefinition, attributesOfInterest);
            for (int i = 0; i < propertyDefinition.Parameters.Count; i++)
            {
                Annotate(propertyDefinition.Parameters[i], annotatedPropertyDefinition.Parameters[i], attributesOfInterest);
            }
        }

        private static void AnnotateField(FieldDefinition fieldDefinition, TypeDefinition annotatedTypeDefinition, Dictionary<string, TypeDefinition> attributesOfInterest)
        {
            var annotatedFieldDefinition = FindMatchingField(fieldDefinition, annotatedTypeDefinition);
            if (annotatedFieldDefinition is null)
                return;

            Annotate(fieldDefinition, annotatedFieldDefinition, attributesOfInterest);
        }

        private static void Annotate(ICustomAttributeProvider provider, ICustomAttributeProvider annotatedProvider, Dictionary<string, TypeDefinition> attributesOfInterest)
        {
            foreach (var customAttribute in annotatedProvider.CustomAttributes)
            {
                if (!attributesOfInterest.TryGetValue(customAttribute.AttributeType.FullName, out var attributeTypeDefinition))
                    continue;

                if (customAttribute.Fields.Count != 0 || customAttribute.Properties.Count != 0)
                    continue;

                var constructor = attributeTypeDefinition.Methods.SingleOrDefault(method => IsMatchingConstructor(method, customAttribute));
                if (constructor is null)
                    continue;

                var newCustomAttribute = new CustomAttribute(constructor);
                for (int i = 0; i < customAttribute.ConstructorArguments.Count; i++)
                {
                    newCustomAttribute.ConstructorArguments.Add(new CustomAttributeArgument(constructor.Parameters[i].ParameterType, customAttribute.ConstructorArguments[i].Value));
                }

                provider.CustomAttributes.Add(newCustomAttribute);
            }

            static bool IsMatchingConstructor(MethodDefinition constructor, CustomAttribute customAttribute)
            {
                if (constructor.Parameters.Count != customAttribute.ConstructorArguments.Count)
                    return false;

                for (int i = 0; i < constructor.Parameters.Count; i++)
                {
                    if (!EquivalenceComparers.TypeReference.Equals(constructor.Parameters[i].ParameterType, customAttribute.ConstructorArguments[i].Type))
                        return false;
                }

                return true;
            }
        }

        private static TypeDefinition? FindMatchingType(TaskLoggingHelper? log, TypeDefinition typeDefinition, ModuleDefinition annotatedModuleDefinition)
        {
            if (typeDefinition.IsNested)
            {
                var declaringAnnotatedType = FindMatchingType(log, typeDefinition.DeclaringType, annotatedModuleDefinition);
                if (declaringAnnotatedType is null)
                    return null;

                return declaringAnnotatedType.NestedTypes.SingleOrDefault(type => EquivalenceComparers.TypeDefinition.Equals(typeDefinition, type));
            }

            var annotatedTypeDefinition = annotatedModuleDefinition.GetType(typeDefinition.Namespace, typeDefinition.Name);
            if (annotatedTypeDefinition is null)
            {
                foreach (var exportedType in annotatedModuleDefinition.ExportedTypes)
                {
                    if (typeDefinition.Name == exportedType.Name && typeDefinition.Namespace == exportedType.Namespace)
                    {
                        try
                        {
                            annotatedTypeDefinition = exportedType.Resolve();
                        }
                        catch (AssemblyResolutionException e)
                        {
                            log?.LogMessage($"Cannot find a matching type for {typeDefinition}");
                            log?.LogMessage(e.Message);
                        }

                        break;
                    }
                }
            }

            if (!EquivalenceComparers.TypeDefinition.Equals(typeDefinition, annotatedTypeDefinition))
                return null;

            return annotatedTypeDefinition;
        }

        private static MethodDefinition FindMatchingMethod(TaskLoggingHelper? log, MethodDefinition methodDefinition, TypeDefinition annotatedTypeDefinition)
        {
            try
            {
                return annotatedTypeDefinition.Methods.SingleOrDefault(annotatedMethod => EquivalenceComparers.MethodDefinition.Equals(methodDefinition, annotatedMethod));
            }
            catch (InvalidOperationException)
            {
                log?.LogMessage($"Cannot find a unique match for {methodDefinition}. Candidates:");
                foreach (var candidate in annotatedTypeDefinition.Methods.Where(annotatedMethod => EquivalenceComparers.MethodDefinition.Equals(methodDefinition, annotatedMethod)))
                {
                    log?.LogMessage($"  {candidate}");
                }

                throw;
            }
        }

        private static PropertyDefinition FindMatchingProperty(PropertyDefinition propertyDefinition, TypeDefinition annotatedTypeDefinition)
        {
            return annotatedTypeDefinition.Properties.SingleOrDefault(property => EquivalenceComparers.PropertyDefinition.Equals(propertyDefinition, property));
        }

        private static FieldDefinition FindMatchingField(FieldDefinition fieldDefinition, TypeDefinition annotatedTypeDefinition)
        {
            return annotatedTypeDefinition.Fields.SingleOrDefault(property => property.Name == fieldDefinition.Name);
        }

        private static TypeDefinition DefineEmbeddedAttribute(AssemblyDefinition assemblyDefinition, WellKnownTypes wellKnownTypes)
        {
            var attribute = new TypeDefinition(
                @namespace: "Microsoft.CodeAnalysis",
                name: "EmbeddedAttribute",
                TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                wellKnownTypes.Module.ImportReference(wellKnownTypes.SystemAttribute));

            var constructor = attribute.AddDefaultConstructor(wellKnownTypes.TypeSystem);

            MethodDefinition compilerGeneratedConstructor = wellKnownTypes.SystemRuntimeCompilerServicesCompilerGeneratedAttribute.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 0);
            var customAttribute = new CustomAttribute(wellKnownTypes.Module.ImportReference(compilerGeneratedConstructor));
            attribute.CustomAttributes.Add(customAttribute);
            attribute.CustomAttributes.Add(new CustomAttribute(constructor));

            assemblyDefinition.MainModule.Types.Add(attribute);

            return attribute;
        }

        private static TypeDefinition DefineReferenceAssemblyAttribute(AssemblyDefinition assemblyDefinition, (TypeReference systemAttribute, TypeReference systemRuntimeCompilerServicesCompilerGeneratedAttribute) wellKnownTypes)
        {
            var attribute = new TypeDefinition(
                @namespace: "System.Runtime.CompilerServices",
                name: "ReferenceAssemblyAttribute",
                TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                assemblyDefinition.MainModule.ImportReference(wellKnownTypes.systemAttribute));

            attribute.AddDefaultConstructor(assemblyDefinition.MainModule.TypeSystem);

            MethodDefinition compilerGeneratedConstructor = wellKnownTypes.systemRuntimeCompilerServicesCompilerGeneratedAttribute.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 0);
            var customAttribute = new CustomAttribute(assemblyDefinition.MainModule.ImportReference(compilerGeneratedConstructor));
            attribute.CustomAttributes.Add(customAttribute);

            assemblyDefinition.MainModule.Types.Add(attribute);

            return attribute;
        }

        private static TypeDefinition DefineNullableAttribute(AssemblyDefinition assemblyDefinition, TypeReference embeddedAttribute, WellKnownTypes wellKnownTypes)
        {
            var attribute = new TypeDefinition(
                @namespace: "System.Runtime.CompilerServices",
                name: "NullableAttribute",
                TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                wellKnownTypes.Module.ImportReference(wellKnownTypes.SystemAttribute));

            MethodDefinition compilerGeneratedConstructor = wellKnownTypes.SystemRuntimeCompilerServicesCompilerGeneratedAttribute.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 0);
            attribute.CustomAttributes.Add(new CustomAttribute(wellKnownTypes.Module.ImportReference(compilerGeneratedConstructor)));
            attribute.CustomAttributes.Add(new CustomAttribute(embeddedAttribute.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 0)));

            var constructorByte = MethodFactory.Constructor(wellKnownTypes.TypeSystem);
            constructorByte.Parameters.Add(new ParameterDefinition(wellKnownTypes.TypeSystem.Byte));

            var constructorByteArray = MethodFactory.Constructor(wellKnownTypes.TypeSystem);
            constructorByteArray.Parameters.Add(new ParameterDefinition(new ArrayType(wellKnownTypes.TypeSystem.Byte)));

            attribute.Methods.Add(constructorByte);
            attribute.Methods.Add(constructorByteArray);

            assemblyDefinition.MainModule.Types.Add(attribute);

            return attribute;
        }

        private static TypeDefinition DefineNullableContextAttribute(AssemblyDefinition assemblyDefinition, TypeReference embeddedAttribute, WellKnownTypes wellKnownTypes)
        {
            var attribute = new TypeDefinition(
                @namespace: "System.Runtime.CompilerServices",
                name: "NullableContextAttribute",
                TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                wellKnownTypes.Module.ImportReference(wellKnownTypes.SystemAttribute));

            MethodDefinition compilerGeneratedConstructor = wellKnownTypes.SystemRuntimeCompilerServicesCompilerGeneratedAttribute.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 0);
            attribute.CustomAttributes.Add(new CustomAttribute(wellKnownTypes.Module.ImportReference(compilerGeneratedConstructor)));
            attribute.CustomAttributes.Add(new CustomAttribute(embeddedAttribute.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 0)));

            var constructor = MethodFactory.Constructor(wellKnownTypes.TypeSystem);
            constructor.Parameters.Add(new ParameterDefinition(wellKnownTypes.TypeSystem.Byte));
            attribute.Methods.Add(constructor);

            assemblyDefinition.MainModule.Types.Add(attribute);

            return attribute;
        }

        private static TypeDefinition DefineNullablePublicOnlyAttribute(AssemblyDefinition assemblyDefinition, TypeReference embeddedAttribute, WellKnownTypes wellKnownTypes)
        {
            var attribute = new TypeDefinition(
                @namespace: "System.Runtime.CompilerServices",
                name: "NullablePublicOnlyAttribute",
                TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                wellKnownTypes.Module.ImportReference(wellKnownTypes.SystemAttribute));

            MethodDefinition compilerGeneratedConstructor = wellKnownTypes.SystemRuntimeCompilerServicesCompilerGeneratedAttribute.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 0);
            attribute.CustomAttributes.Add(new CustomAttribute(wellKnownTypes.Module.ImportReference(compilerGeneratedConstructor)));
            attribute.CustomAttributes.Add(new CustomAttribute(embeddedAttribute.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 0)));

            var constructor = MethodFactory.Constructor(wellKnownTypes.TypeSystem);
            constructor.Parameters.Add(new ParameterDefinition(wellKnownTypes.TypeSystem.Boolean));
            attribute.Methods.Add(constructor);

            assemblyDefinition.MainModule.Types.Add(attribute);

            return attribute;
        }

        private static TypeDefinition DefineAllowNullAttribute(AssemblyDefinition assemblyDefinition, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
        {
            var attribute = new TypeDefinition(
                @namespace: "System.Diagnostics.CodeAnalysis",
                name: "AllowNullAttribute",
                TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                wellKnownTypes.Module.ImportReference(wellKnownTypes.SystemAttribute));

            attribute.AddDefaultConstructor(wellKnownTypes.TypeSystem);
            attribute.CustomAttributes.Add(attributeFactory.AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, inherited: false));

            assemblyDefinition.MainModule.Types.Add(attribute);

            return attribute;
        }

        private static TypeDefinition DefineDisallowNullAttribute(AssemblyDefinition assemblyDefinition, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
        {
            var attribute = new TypeDefinition(
                @namespace: "System.Diagnostics.CodeAnalysis",
                name: "DisallowNullAttribute",
                TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                wellKnownTypes.Module.ImportReference(wellKnownTypes.SystemAttribute));

            attribute.AddDefaultConstructor(wellKnownTypes.TypeSystem);
            attribute.CustomAttributes.Add(attributeFactory.AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, inherited: false));

            assemblyDefinition.MainModule.Types.Add(attribute);

            return attribute;
        }

        private static TypeDefinition DefineDoesNotReturnAttribute(AssemblyDefinition assemblyDefinition, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
        {
            var attribute = new TypeDefinition(
                @namespace: "System.Diagnostics.CodeAnalysis",
                name: "DoesNotReturnAttribute",
                TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                wellKnownTypes.Module.ImportReference(wellKnownTypes.SystemAttribute));

            attribute.AddDefaultConstructor(wellKnownTypes.TypeSystem);
            attribute.CustomAttributes.Add(attributeFactory.AttributeUsage(AttributeTargets.Method, inherited: false));

            assemblyDefinition.MainModule.Types.Add(attribute);

            return attribute;
        }

        private static TypeDefinition DefineDoesNotReturnIfAttribute(AssemblyDefinition assemblyDefinition, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
        {
            var attribute = new TypeDefinition(
                @namespace: "System.Diagnostics.CodeAnalysis",
                name: "DoesNotReturnIfAttribute",
                TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                wellKnownTypes.Module.ImportReference(wellKnownTypes.SystemAttribute));

            var constructor = MethodFactory.Constructor(wellKnownTypes.TypeSystem);
            constructor.Parameters.Add(new ParameterDefinition("parameterValue", ParameterAttributes.None, wellKnownTypes.TypeSystem.Boolean));
            attribute.Methods.Add(constructor);

            attribute.CustomAttributes.Add(attributeFactory.AttributeUsage(AttributeTargets.Parameter, inherited: false));

            assemblyDefinition.MainModule.Types.Add(attribute);

            return attribute;
        }

        private static TypeDefinition DefineMaybeNullAttribute(AssemblyDefinition assemblyDefinition, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
        {
            var attribute = new TypeDefinition(
                @namespace: "System.Diagnostics.CodeAnalysis",
                name: "MaybeNullAttribute",
                TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                wellKnownTypes.Module.ImportReference(wellKnownTypes.SystemAttribute));

            attribute.AddDefaultConstructor(wellKnownTypes.TypeSystem);
            attribute.CustomAttributes.Add(attributeFactory.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, inherited: false));

            assemblyDefinition.MainModule.Types.Add(attribute);

            return attribute;
        }

        private static TypeDefinition DefineMaybeNullWhenAttribute(AssemblyDefinition assemblyDefinition, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
        {
            var attribute = new TypeDefinition(
                @namespace: "System.Diagnostics.CodeAnalysis",
                name: "MaybeNullWhenAttribute",
                TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                wellKnownTypes.Module.ImportReference(wellKnownTypes.SystemAttribute));

            var constructor = MethodFactory.Constructor(wellKnownTypes.TypeSystem);
            constructor.Parameters.Add(new ParameterDefinition("returnValue", ParameterAttributes.None, wellKnownTypes.TypeSystem.Boolean));
            attribute.Methods.Add(constructor);

            attribute.CustomAttributes.Add(attributeFactory.AttributeUsage(AttributeTargets.Parameter, inherited: false));

            assemblyDefinition.MainModule.Types.Add(attribute);

            return attribute;
        }

        private static TypeDefinition DefineNotNullAttribute(AssemblyDefinition assemblyDefinition, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
        {
            var attribute = new TypeDefinition(
                @namespace: "System.Diagnostics.CodeAnalysis",
                name: "NotNullAttribute",
                TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                wellKnownTypes.Module.ImportReference(wellKnownTypes.SystemAttribute));

            attribute.AddDefaultConstructor(wellKnownTypes.TypeSystem);
            attribute.CustomAttributes.Add(attributeFactory.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, inherited: false));

            assemblyDefinition.MainModule.Types.Add(attribute);

            return attribute;
        }

        private static TypeDefinition DefineNotNullIfNotNullAttribute(AssemblyDefinition assemblyDefinition, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
        {
            var attribute = new TypeDefinition(
                @namespace: "System.Diagnostics.CodeAnalysis",
                name: "NotNullIfNotNullAttribute",
                TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                wellKnownTypes.Module.ImportReference(wellKnownTypes.SystemAttribute));

            attribute.CustomAttributes.Add(attributeFactory.NullableContext(1));
            attribute.CustomAttributes.Add(attributeFactory.Nullable(0));
            attribute.CustomAttributes.Add(attributeFactory.AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, allowMultiple: true, inherited: false));

            var constructor = MethodFactory.Constructor(wellKnownTypes.TypeSystem);
            constructor.Parameters.Add(new ParameterDefinition("parameterName", ParameterAttributes.None, wellKnownTypes.TypeSystem.String));
            attribute.Methods.Add(constructor);

            assemblyDefinition.MainModule.Types.Add(attribute);

            return attribute;
        }

        private static TypeDefinition DefineNotNullWhenAttribute(AssemblyDefinition assemblyDefinition, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
        {
            var attribute = new TypeDefinition(
                @namespace: "System.Diagnostics.CodeAnalysis",
                name: "NotNullWhenAttribute",
                TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                wellKnownTypes.Module.ImportReference(wellKnownTypes.SystemAttribute));

            var constructor = MethodFactory.Constructor(wellKnownTypes.TypeSystem);
            constructor.Parameters.Add(new ParameterDefinition("returnValue", ParameterAttributes.None, wellKnownTypes.TypeSystem.Boolean));
            attribute.Methods.Add(constructor);

            attribute.CustomAttributes.Add(attributeFactory.AttributeUsage(AttributeTargets.Parameter, inherited: false));

            assemblyDefinition.MainModule.Types.Add(attribute);

            return attribute;
        }

        private static void EnsureReferenceAssemblyAttribute(AssemblyDefinition assemblyDefinition, CustomAttributeFactory attributeFactory)
        {
            foreach (var attribute in assemblyDefinition.CustomAttributes)
            {
                if (attribute.AttributeType.FullName == typeof(ReferenceAssemblyAttribute).FullName)
                {
                    return;
                }
            }

            var customAttribute = attributeFactory.ReferenceAssembly();
            assemblyDefinition.CustomAttributes.Add(customAttribute);
        }
    }
}

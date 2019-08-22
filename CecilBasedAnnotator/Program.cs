// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CecilBasedAnnotator
{
    class Program
    {
        static void Main(string[] args)
        {
            string referenceAssembly = args[0];
            string annotatedReferenceAssembly = args[1];
            string outputAssembly = args[2];

            var assemblyResolver = new DefaultAssemblyResolver();
            assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(referenceAssembly));
            using var assemblyDefinition = AssemblyDefinition.ReadAssembly(referenceAssembly, new ReaderParameters(ReadingMode.Immediate) { AssemblyResolver = assemblyResolver });

            var annotatedAssemblyResolver = new DefaultAssemblyResolver();
            annotatedAssemblyResolver.AddSearchDirectory(Path.GetDirectoryName(annotatedReferenceAssembly));
            using var annotatedAssemblyDefinition = AssemblyDefinition.ReadAssembly(annotatedReferenceAssembly, new ReaderParameters(ReadingMode.Immediate) { AssemblyResolver = annotatedAssemblyResolver });

            var wellKnownTypes = new WellKnownTypes(assemblyDefinition.MainModule);

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
            attributesOfInterest.Add(notNullIfNotNullAttribute.FullName,notNullIfNotNullAttribute);
            attributesOfInterest.Add(notNullWhenAttribute.FullName, notNullWhenAttribute);

            AnnotateAssembly(assemblyDefinition, annotatedAssemblyDefinition, attributesOfInterest);

            assemblyDefinition.Write(outputAssembly);
        }

        private static void AnnotateAssembly(AssemblyDefinition assemblyDefinition, AssemblyDefinition annotatedAssemblyDefinition, Dictionary<string, TypeDefinition> attributesOfInterest)
        {
            Annotate(assemblyDefinition, annotatedAssemblyDefinition, attributesOfInterest);
            if (assemblyDefinition.Modules.Count != 1)
                throw new NotSupportedException();

            AnnotateModule(assemblyDefinition.MainModule, annotatedAssemblyDefinition.MainModule, attributesOfInterest);
        }

        private static void AnnotateModule(ModuleDefinition moduleDefinition, ModuleDefinition annotatedModuleDefinition, Dictionary<string, TypeDefinition> attributesOfInterest)
        {
            Annotate(moduleDefinition, annotatedModuleDefinition, attributesOfInterest);
            foreach (var type in moduleDefinition.GetAllTypes())
            {
                AnnotateType(type, annotatedModuleDefinition, attributesOfInterest);
            }
        }

        private static void AnnotateType(TypeDefinition typeDefinition, ModuleDefinition annotatedModuleDefinition, Dictionary<string, TypeDefinition> attributesOfInterest)
        {
            if (attributesOfInterest.ContainsKey(typeDefinition.FullName))
                return;

            var annotatedTypeDefinition = FindMatchingType(typeDefinition, annotatedModuleDefinition);
            if (annotatedTypeDefinition is null)
            {
                return;
            }

            Annotate(typeDefinition, annotatedTypeDefinition, attributesOfInterest);

            for (var i = 0; i < typeDefinition.GenericParameters.Count; i++)
            {
                Annotate(typeDefinition.GenericParameters[i], annotatedTypeDefinition.GenericParameters[i], attributesOfInterest);
            }

            foreach (var method in typeDefinition.Methods)
            {
                AnnotateMethod(method, annotatedTypeDefinition, attributesOfInterest);
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

        private static void AnnotateMethod(MethodDefinition methodDefinition, TypeDefinition annotatedTypeDefinition, Dictionary<string, TypeDefinition> attributesOfInterest)
        {
            var annotatedMethodDefinition = FindMatchingMethod(methodDefinition, annotatedTypeDefinition);
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

                if (attributeTypeDefinition.Methods.Count != 1)
                    continue;

                if (customAttribute.Fields.Count != 0 || customAttribute.Properties.Count != 0)
                    continue;

                var constructor = attributeTypeDefinition.Methods.Single();
                var newCustomAttribute = new CustomAttribute(constructor);
                for (var i = 0; i < customAttribute.ConstructorArguments.Count; i++)
                {
                    newCustomAttribute.ConstructorArguments.Add(new CustomAttributeArgument(constructor.Parameters[i].ParameterType, customAttribute.ConstructorArguments[i].Value));
                }

                provider.CustomAttributes.Add(newCustomAttribute);
            }
        }

        private static TypeDefinition FindMatchingType(TypeDefinition typeDefinition, ModuleDefinition annotatedModuleDefinition)
        {
            if (typeDefinition.IsNested)
            {
                var declaringAnnotatedType = FindMatchingType(typeDefinition.DeclaringType, annotatedModuleDefinition);
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
                            Console.WriteLine($"Cannot find a matching type for {typeDefinition}");
                            Console.WriteLine(e.Message);
                        }

                        break;
                    }
                }
            }

            if (!EquivalenceComparers.TypeDefinition.Equals(typeDefinition, annotatedTypeDefinition))
                return null;

            return annotatedTypeDefinition;
        }

        private static MethodDefinition FindMatchingMethod(MethodDefinition methodDefinition, TypeDefinition annotatedTypeDefinition)
        {
            try
            {
                return annotatedTypeDefinition.Methods.SingleOrDefault(annotatedMethod => EquivalenceComparers.MethodDefinition.Equals(methodDefinition, annotatedMethod));
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine($"Cannot find a unique match for {methodDefinition}. Candidates:");
                foreach (var candidate in annotatedTypeDefinition.Methods.Where(annotatedMethod => EquivalenceComparers.MethodDefinition.Equals(methodDefinition, annotatedMethod)))
                {
                    Console.WriteLine($"  {candidate}");
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

            var constructor = attribute.AddDefaultConstructor(wellKnownTypes);

            MethodDefinition compilerGeneratedConstructor = wellKnownTypes.SystemRuntimeCompilerServicesCompilerGeneratedAttribute.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 0);
            var customAttribute = new CustomAttribute(wellKnownTypes.Module.ImportReference(compilerGeneratedConstructor));
            attribute.CustomAttributes.Add(customAttribute);
            attribute.CustomAttributes.Add(new CustomAttribute(constructor));

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

            var constructorByte = MethodFactory.Constructor(wellKnownTypes);
            constructorByte.Parameters.Add(new ParameterDefinition(wellKnownTypes.TypeSystem.Byte));

            var constructorByteArray = MethodFactory.Constructor(wellKnownTypes);
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

            var constructor = MethodFactory.Constructor(wellKnownTypes);
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

            var constructor = MethodFactory.Constructor(wellKnownTypes);
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

            attribute.AddDefaultConstructor(wellKnownTypes);
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

            attribute.AddDefaultConstructor(wellKnownTypes);
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

            attribute.AddDefaultConstructor(wellKnownTypes);
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

            var constructor = MethodFactory.Constructor(wellKnownTypes);
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

            attribute.AddDefaultConstructor(wellKnownTypes);
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

            var constructor = MethodFactory.Constructor(wellKnownTypes);
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

            attribute.AddDefaultConstructor(wellKnownTypes);
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

            var constructor = MethodFactory.Constructor(wellKnownTypes);
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

            var constructor = MethodFactory.Constructor(wellKnownTypes);
            constructor.Parameters.Add(new ParameterDefinition("returnValue", ParameterAttributes.None, wellKnownTypes.TypeSystem.Boolean));
            attribute.Methods.Add(constructor);

            attribute.CustomAttributes.Add(attributeFactory.AttributeUsage(AttributeTargets.Parameter, inherited: false));

            assemblyDefinition.MainModule.Types.Add(attribute);

            return attribute;
        }
    }
}

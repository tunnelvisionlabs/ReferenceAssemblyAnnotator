// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Mono.Cecil;
    using Mono.Cecil.Rocks;

    internal class Program
    {
        internal static void Main(SuppressibleLoggingHelper? log, string referenceAssembly, string annotatedReferenceAssembly, string outputAssembly)
        {
            var assemblyResolver = new DefaultAssemblyResolver();
            assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(referenceAssembly));
            using var assemblyDefinition = AssemblyDefinition.ReadAssembly(referenceAssembly, new ReaderParameters(ReadingMode.Deferred) { AssemblyResolver = assemblyResolver });

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
            using var annotatedAssemblyDefinition = AssemblyDefinition.ReadAssembly(annotatedReferenceAssembly, new ReaderParameters(ReadingMode.Deferred) { AssemblyResolver = annotatedAssemblyResolver });

            var wellKnownTypes = new WellKnownTypes(assemblyDefinition);

            var attributeFactory = new CustomAttributeFactory(wellKnownTypes);

            // Ensure the assembly is marked with ReferenceAssemblyAttribute
            EnsureReferenceAssemblyAttribute(assemblyDefinition, attributeFactory);

            var attributesOfInterest = new Dictionary<string, TypeDefinition>();
            AddAttributeOfInterest(attributesOfInterest, wellKnownTypes.SystemRuntimeCompilerServicesNullableAttribute);
            AddAttributeOfInterest(attributesOfInterest, wellKnownTypes.SystemRuntimeCompilerServicesNullableContextAttribute);
            AddAttributeOfInterest(attributesOfInterest, wellKnownTypes.SystemRuntimeCompilerServicesNullablePublicOnlyAttribute);
            AddAttributeOfInterest(attributesOfInterest, wellKnownTypes.SystemDiagnosticsCodeAnalysisAllowNullAttribute);
            AddAttributeOfInterest(attributesOfInterest, wellKnownTypes.SystemDiagnosticsCodeAnalysisDisallowNullAttribute);
            AddAttributeOfInterest(attributesOfInterest, wellKnownTypes.SystemDiagnosticsCodeAnalysisDoesNotReturnAttribute);
            AddAttributeOfInterest(attributesOfInterest, wellKnownTypes.SystemDiagnosticsCodeAnalysisDoesNotReturnIfAttribute);
            AddAttributeOfInterest(attributesOfInterest, wellKnownTypes.SystemDiagnosticsCodeAnalysisMaybeNullAttribute);
            AddAttributeOfInterest(attributesOfInterest, wellKnownTypes.SystemDiagnosticsCodeAnalysisMaybeNullWhenAttribute);
            AddAttributeOfInterest(attributesOfInterest, wellKnownTypes.SystemDiagnosticsCodeAnalysisNotNullAttribute);
            AddAttributeOfInterest(attributesOfInterest, wellKnownTypes.SystemDiagnosticsCodeAnalysisNotNullIfNotNullAttribute);
            AddAttributeOfInterest(attributesOfInterest, wellKnownTypes.SystemDiagnosticsCodeAnalysisNotNullWhenAttribute);

            AnnotateAssembly(log, assemblyDefinition, annotatedAssemblyDefinition, attributesOfInterest);

            assemblyDefinition.Write(outputAssembly);

            return;

            // Local functions
            static void AddAttributeOfInterest(Dictionary<string, TypeDefinition> attributesOfInterest, Lazy<TypeReference> typeReference)
            {
                attributesOfInterest.Add(typeReference.Value.FullName, typeReference.Value.Resolve());
            }
        }

        private static void AnnotateAssembly(SuppressibleLoggingHelper? log, AssemblyDefinition assemblyDefinition, AssemblyDefinition annotatedAssemblyDefinition, Dictionary<string, TypeDefinition> attributesOfInterest)
        {
            Annotate(assemblyDefinition.MainModule, assemblyDefinition, annotatedAssemblyDefinition, attributesOfInterest);
            if (assemblyDefinition.Modules.Count != 1)
                throw new NotSupportedException();

            AnnotateModule(log, assemblyDefinition.MainModule, annotatedAssemblyDefinition.MainModule, attributesOfInterest);
        }

        private static void AnnotateModule(SuppressibleLoggingHelper? log, ModuleDefinition moduleDefinition, ModuleDefinition annotatedModuleDefinition, Dictionary<string, TypeDefinition> attributesOfInterest)
        {
            Annotate(moduleDefinition, moduleDefinition, annotatedModuleDefinition, attributesOfInterest);
            foreach (var type in moduleDefinition.GetAllTypes())
            {
                AnnotateType(log, moduleDefinition, type, annotatedModuleDefinition, attributesOfInterest);
            }
        }

        private static void AnnotateType(SuppressibleLoggingHelper? log, ModuleDefinition module, TypeDefinition typeDefinition, ModuleDefinition annotatedModuleDefinition, Dictionary<string, TypeDefinition> attributesOfInterest)
        {
            if (attributesOfInterest.ContainsKey(typeDefinition.FullName))
                return;

            var annotatedTypeDefinition = FindMatchingType(log, typeDefinition, annotatedModuleDefinition);
            if (annotatedTypeDefinition is null)
            {
                return;
            }

            Annotate(module, typeDefinition, annotatedTypeDefinition, attributesOfInterest);
            for (int i = 0; i < typeDefinition.Interfaces.Count; i++)
            {
                for (int j = 0; j < annotatedTypeDefinition.Interfaces.Count; j++)
                {
                    if (EquivalenceComparers.TypeReference.Equals(typeDefinition.Interfaces[i].InterfaceType, annotatedTypeDefinition.Interfaces[j].InterfaceType))
                    {
                        Annotate(module, typeDefinition.Interfaces[i], annotatedTypeDefinition.Interfaces[j], attributesOfInterest);
                    }
                }
            }

            for (int i = 0; i < typeDefinition.GenericParameters.Count; i++)
            {
                Annotate(module, typeDefinition.GenericParameters[i], annotatedTypeDefinition.GenericParameters[i], attributesOfInterest);
            }

            foreach (var method in typeDefinition.Methods)
            {
                AnnotateMethod(log, module, method, annotatedTypeDefinition, attributesOfInterest);
            }

            foreach (var property in typeDefinition.Properties)
            {
                AnnotateProperty(module, property, annotatedTypeDefinition, attributesOfInterest);
            }

            foreach (var field in typeDefinition.Fields)
            {
                AnnotateField(module, field, annotatedTypeDefinition, attributesOfInterest);
            }
        }

        private static void AnnotateMethod(SuppressibleLoggingHelper? log, ModuleDefinition module, MethodDefinition methodDefinition, TypeDefinition annotatedTypeDefinition, Dictionary<string, TypeDefinition> attributesOfInterest)
        {
            var annotatedMethodDefinition = FindMatchingMethod(log, methodDefinition, annotatedTypeDefinition);
            if (annotatedMethodDefinition is null)
                return;

            Annotate(module, methodDefinition, annotatedMethodDefinition, attributesOfInterest);
            Annotate(module, methodDefinition.MethodReturnType, annotatedMethodDefinition.MethodReturnType, attributesOfInterest);
            for (int i = 0; i < methodDefinition.Parameters.Count; i++)
            {
                Annotate(module, methodDefinition.Parameters[i], annotatedMethodDefinition.Parameters[i], attributesOfInterest);
            }

            for (int i = 0; i < methodDefinition.GenericParameters.Count; i++)
            {
                Annotate(module, methodDefinition.GenericParameters[i], annotatedMethodDefinition.GenericParameters[i], attributesOfInterest);
            }
        }

        private static void AnnotateProperty(ModuleDefinition module, PropertyDefinition propertyDefinition, TypeDefinition annotatedTypeDefinition, Dictionary<string, TypeDefinition> attributesOfInterest)
        {
            var annotatedPropertyDefinition = FindMatchingProperty(propertyDefinition, annotatedTypeDefinition);
            if (annotatedPropertyDefinition is null)
                return;

            Annotate(module, propertyDefinition, annotatedPropertyDefinition, attributesOfInterest);
            for (int i = 0; i < propertyDefinition.Parameters.Count; i++)
            {
                Annotate(module, propertyDefinition.Parameters[i], annotatedPropertyDefinition.Parameters[i], attributesOfInterest);
            }
        }

        private static void AnnotateField(ModuleDefinition module, FieldDefinition fieldDefinition, TypeDefinition annotatedTypeDefinition, Dictionary<string, TypeDefinition> attributesOfInterest)
        {
            var annotatedFieldDefinition = FindMatchingField(fieldDefinition, annotatedTypeDefinition);
            if (annotatedFieldDefinition is null)
                return;

            Annotate(module, fieldDefinition, annotatedFieldDefinition, attributesOfInterest);
        }

        private static void Annotate(ModuleDefinition module, ICustomAttributeProvider provider, ICustomAttributeProvider annotatedProvider, Dictionary<string, TypeDefinition> attributesOfInterest)
        {
            // Start by removing excluded attributes as well as attributes of interest to allow, say, rewriting
            // netcoreapp3.1 to use .NET 5 reference annotations.
            for (int i = 0; i < provider.CustomAttributes.Count; i++)
            {
                var customAttribute = provider.CustomAttributes[i];

                if (attributesOfInterest.ContainsKey(customAttribute.AttributeType.FullName)
                    || IsExcludedAnnotation(provider, annotatedProvider, customAttribute))
                {
                    provider.CustomAttributes.RemoveAt(i);
                    i--;
                }
            }

            foreach (var customAttribute in annotatedProvider.CustomAttributes)
            {
                if (!attributesOfInterest.TryGetValue(customAttribute.AttributeType.FullName, out var attributeTypeDefinition))
                    continue;

                if (customAttribute.Fields.Count != 0 || customAttribute.Properties.Count != 0)
                    continue;

                if (IsExcludedAnnotation(provider, annotatedProvider, customAttribute))
                    continue;

                var constructor = attributeTypeDefinition.Methods.SingleOrDefault(method => IsMatchingConstructor(method, customAttribute));
                if (constructor is null)
                    continue;

                var newCustomAttribute = new CustomAttribute(module.ImportReference(constructor));
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

        private static bool IsExcludedAnnotation(ICustomAttributeProvider provider, ICustomAttributeProvider annotatedProvider, CustomAttribute customAttribute)
        {
            if (provider is ParameterDefinition parameter
                && parameter.Method is MethodDefinition { Name: nameof(GetHashCode), Parameters: { Count: 1 } } method
                && method.DeclaringType is { Namespace: "System.Collections.Generic", GenericParameters: { Count: 1 } } type)
            {
                if (type.Name == "IEqualityComparer`1"
                    || type.Name == "EqualityComparer`1")
                {
                    // Remove DisallowNullAttribute from:
                    // 1. System.Collections.Generic.IEqualityComparer<T>.GetHashCode([DisallowNull] T)
                    // 2. System.Collections.Generic.EqualityComparer<T>.GetHashCode([DisallowNull] T)
                    return customAttribute.AttributeType.FullName == "System.Diagnostics.CodeAnalysis.DisallowNullAttribute";
                }
            }

            return false;
        }

        private static TypeDefinition? FindMatchingType(SuppressibleLoggingHelper? log, TypeDefinition typeDefinition, ModuleDefinition annotatedModuleDefinition)
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

        private static MethodDefinition FindMatchingMethod(SuppressibleLoggingHelper? log, MethodDefinition methodDefinition, TypeDefinition annotatedTypeDefinition)
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

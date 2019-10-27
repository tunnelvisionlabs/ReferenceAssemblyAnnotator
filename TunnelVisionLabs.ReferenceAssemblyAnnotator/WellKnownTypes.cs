// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Mono.Cecil;

    internal partial class WellKnownTypes
    {
        private readonly WellKnownType _systemAttribute = new PredefinedType(typeof(Attribute));
        private readonly WellKnownType _systemAttributeTargets = new PredefinedType(typeof(AttributeTargets));
        private readonly WellKnownType _systemAttributeUsageAttribute = new PredefinedType(typeof(AttributeUsageAttribute));
        private readonly WellKnownType _systemRuntimeCompilerServicesCompilerGeneratedAttribute = new PredefinedType(typeof(CompilerGeneratedAttribute));

        private readonly WellKnownType _systemRuntimeCompilerServicesReferenceAssemblyAttribute = new ReferenceAssemblyAttributeProvidedType();
        private readonly WellKnownType _microsoftCodeAnalysisEmbeddedAttribute = new EmbeddedAttributeProvidedType();
        private readonly WellKnownType _systemRuntimeCompilerServicesNullableAttribute = new NullableAttributeProvidedType();
        private readonly WellKnownType _systemRuntimeCompilerServicesNullableContextAttribute = new NullableContextAttributeProvidedType();
        private readonly WellKnownType _systemRuntimeCompilerServicesNullablePublicOnlyAttribute = new NullablePublicOnlyAttributeProvidedType();

        public WellKnownTypes(AssemblyDefinition assemblyDefinition)
        {
            Module = assemblyDefinition.MainModule;

            SystemRuntimeCompilerServicesReferenceAssemblyAttribute = new Lazy<TypeReference>(
                () => _systemRuntimeCompilerServicesReferenceAssemblyAttribute.GetOrCreateTypeReference(Module, this),
                LazyThreadSafetyMode.ExecutionAndPublication);
            MicrosoftCodeAnalysisEmbeddedAttribute = new Lazy<TypeReference>(
                () => _microsoftCodeAnalysisEmbeddedAttribute.GetOrCreateTypeReference(Module, this),
                LazyThreadSafetyMode.ExecutionAndPublication);
            SystemRuntimeCompilerServicesNullableAttribute = new Lazy<TypeReference>(
                () => _systemRuntimeCompilerServicesNullableAttribute.GetOrCreateTypeReference(Module, this),
                LazyThreadSafetyMode.ExecutionAndPublication);
            SystemRuntimeCompilerServicesNullableContextAttribute = new Lazy<TypeReference>(
                () => _systemRuntimeCompilerServicesNullableContextAttribute.GetOrCreateTypeReference(Module, this),
                LazyThreadSafetyMode.ExecutionAndPublication);
            SystemRuntimeCompilerServicesNullablePublicOnlyAttribute = new Lazy<TypeReference>(
                () => _systemRuntimeCompilerServicesNullablePublicOnlyAttribute.GetOrCreateTypeReference(Module, this),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public ModuleDefinition Module { get; }

        public TypeSystem TypeSystem => Module.TypeSystem;

        public TypeReference SystemAttribute => _systemAttribute.GetOrCreateTypeReference(Module, this);

        public TypeReference SystemAttributeTargets => _systemAttributeTargets.GetOrCreateTypeReference(Module, this);

        public TypeReference SystemAttributeUsageAttribute => _systemAttributeUsageAttribute.GetOrCreateTypeReference(Module, this);

        public TypeReference SystemRuntimeCompilerServicesCompilerGeneratedAttribute => _systemRuntimeCompilerServicesCompilerGeneratedAttribute.GetOrCreateTypeReference(Module, this);

        public Lazy<TypeReference> SystemRuntimeCompilerServicesReferenceAssemblyAttribute { get; }

        public Lazy<TypeReference> MicrosoftCodeAnalysisEmbeddedAttribute { get; }

        public Lazy<TypeReference> SystemRuntimeCompilerServicesNullableAttribute { get; }

        public Lazy<TypeReference> SystemRuntimeCompilerServicesNullableContextAttribute { get; }

        public Lazy<TypeReference> SystemRuntimeCompilerServicesNullablePublicOnlyAttribute { get; }

        private static TypeDefinition ResolveRequiredWellKnownType(ModuleDefinition module, Type type)
        {
            return ResolveWellKnownType(module, type)
                ?? throw new NotSupportedException($"Failed to resolve type '{type.FullName}'");
        }

        private static TypeDefinition? ResolveWellKnownType(ModuleDefinition module, Type type)
        {
            return module.TypeSystem.Object.Resolve().Module.GetType(type.FullName);
        }
    }
}

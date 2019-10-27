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

        private readonly WellKnownType _systemDiagnosticsCodeAnalysisAllowNullAttribute = new AllowNullAttributeProvidedType();
        private readonly WellKnownType _systemDiagnosticsCodeAnalysisDisallowNullAttribute = new DisallowNullAttributeProvidedType();
        private readonly WellKnownType _systemDiagnosticsCodeAnalysisDoesNotReturnAttribute = new DoesNotReturnAttributeProvidedType();
        private readonly WellKnownType _systemDiagnosticsCodeAnalysisDoesNotReturnIfAttribute = new DoesNotReturnIfAttributeProvidedType();
        private readonly WellKnownType _systemDiagnosticsCodeAnalysisMaybeNullAttribute = new MaybeNullAttributeProvidedType();
        private readonly WellKnownType _systemDiagnosticsCodeAnalysisMaybeNullWhenAttribute = new MaybeNullWhenAttributeProvidedType();
        private readonly WellKnownType _systemDiagnosticsCodeAnalysisNotNullAttribute = new NotNullAttributeProvidedType();
        private readonly WellKnownType _systemDiagnosticsCodeAnalysisNotNullIfNotNullAttribute = new NotNullIfNotNullAttributeProvidedType();
        private readonly WellKnownType _systemDiagnosticsCodeAnalysisNotNullWhenAttribute = new NotNullWhenAttributeProvidedType();

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

            SystemDiagnosticsCodeAnalysisAllowNullAttribute = new Lazy<TypeReference>(
                () => _systemDiagnosticsCodeAnalysisAllowNullAttribute.GetOrCreateTypeReference(Module, this),
                LazyThreadSafetyMode.ExecutionAndPublication);
            SystemDiagnosticsCodeAnalysisDisallowNullAttribute = new Lazy<TypeReference>(
                () => _systemDiagnosticsCodeAnalysisDisallowNullAttribute.GetOrCreateTypeReference(Module, this),
                LazyThreadSafetyMode.ExecutionAndPublication);
            SystemDiagnosticsCodeAnalysisDoesNotReturnAttribute = new Lazy<TypeReference>(
                () => _systemDiagnosticsCodeAnalysisDoesNotReturnAttribute.GetOrCreateTypeReference(Module, this),
                LazyThreadSafetyMode.ExecutionAndPublication);
            SystemDiagnosticsCodeAnalysisDoesNotReturnIfAttribute = new Lazy<TypeReference>(
                () => _systemDiagnosticsCodeAnalysisDoesNotReturnIfAttribute.GetOrCreateTypeReference(Module, this),
                LazyThreadSafetyMode.ExecutionAndPublication);
            SystemDiagnosticsCodeAnalysisMaybeNullAttribute = new Lazy<TypeReference>(
                () => _systemDiagnosticsCodeAnalysisMaybeNullAttribute.GetOrCreateTypeReference(Module, this),
                LazyThreadSafetyMode.ExecutionAndPublication);
            SystemDiagnosticsCodeAnalysisMaybeNullWhenAttribute = new Lazy<TypeReference>(
                () => _systemDiagnosticsCodeAnalysisMaybeNullWhenAttribute.GetOrCreateTypeReference(Module, this),
                LazyThreadSafetyMode.ExecutionAndPublication);
            SystemDiagnosticsCodeAnalysisNotNullAttribute = new Lazy<TypeReference>(
                () => _systemDiagnosticsCodeAnalysisNotNullAttribute.GetOrCreateTypeReference(Module, this),
                LazyThreadSafetyMode.ExecutionAndPublication);
            SystemDiagnosticsCodeAnalysisNotNullIfNotNullAttribute = new Lazy<TypeReference>(
                () => _systemDiagnosticsCodeAnalysisNotNullIfNotNullAttribute.GetOrCreateTypeReference(Module, this),
                LazyThreadSafetyMode.ExecutionAndPublication);
            SystemDiagnosticsCodeAnalysisNotNullWhenAttribute = new Lazy<TypeReference>(
                () => _systemDiagnosticsCodeAnalysisNotNullWhenAttribute.GetOrCreateTypeReference(Module, this),
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

        public Lazy<TypeReference> SystemDiagnosticsCodeAnalysisAllowNullAttribute { get; }

        public Lazy<TypeReference> SystemDiagnosticsCodeAnalysisDisallowNullAttribute { get; }

        public Lazy<TypeReference> SystemDiagnosticsCodeAnalysisDoesNotReturnAttribute { get; }

        public Lazy<TypeReference> SystemDiagnosticsCodeAnalysisDoesNotReturnIfAttribute { get; }

        public Lazy<TypeReference> SystemDiagnosticsCodeAnalysisMaybeNullAttribute { get; }

        public Lazy<TypeReference> SystemDiagnosticsCodeAnalysisMaybeNullWhenAttribute { get; }

        public Lazy<TypeReference> SystemDiagnosticsCodeAnalysisNotNullAttribute { get; }

        public Lazy<TypeReference> SystemDiagnosticsCodeAnalysisNotNullIfNotNullAttribute { get; }

        public Lazy<TypeReference> SystemDiagnosticsCodeAnalysisNotNullWhenAttribute { get; }

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

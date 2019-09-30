// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System;
    using System.Runtime.CompilerServices;
    using Mono.Cecil;

    internal class WellKnownTypes
    {
        public WellKnownTypes(ModuleDefinition module)
        {
            Module = module;

            SystemAttribute = ResolveRequiredWellKnownType(module, typeof(Attribute));
            SystemAttributeTargets = ResolveRequiredWellKnownType(module, typeof(AttributeTargets));
            SystemAttributeUsageAttribute = ResolveRequiredWellKnownType(module, typeof(AttributeUsageAttribute));
            SystemRuntimeCompilerServicesCompilerGeneratedAttribute = ResolveRequiredWellKnownType(module, typeof(CompilerGeneratedAttribute));
            SystemRuntimeCompilerServicesReferenceAssemblyAttribute = ResolveWellKnownType(module, typeof(ReferenceAssemblyAttribute));
        }

        public ModuleDefinition Module { get; }

        public TypeSystem TypeSystem => Module.TypeSystem;

        public TypeReference SystemAttribute { get; }

        public TypeReference SystemAttributeTargets { get; }

        public TypeReference SystemAttributeUsageAttribute { get; }

        public TypeReference SystemRuntimeCompilerServicesCompilerGeneratedAttribute { get; }

        public TypeReference? SystemRuntimeCompilerServicesReferenceAssemblyAttribute { get; }

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

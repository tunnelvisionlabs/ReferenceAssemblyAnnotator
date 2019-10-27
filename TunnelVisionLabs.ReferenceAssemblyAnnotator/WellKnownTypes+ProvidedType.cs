// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using Mono.Cecil;

    internal partial class WellKnownTypes
    {
        private abstract class ProvidedType : WellKnownType
        {
            protected ProvidedType(string? namespaceName, string typeName)
            {
                NamespaceName = namespaceName;
                TypeName = typeName;
                FullTypeName = string.IsNullOrEmpty(namespaceName) ? typeName : $"{NamespaceName}.{TypeName}";
            }

            public string? NamespaceName { get; }

            public string TypeName { get; }

            public string FullTypeName { get; }

            protected override TypeReference GetOrCreateTypeReferenceImpl(ModuleDefinition module, WellKnownTypes wellKnownTypes)
            {
                var existingType = module.TypeSystem.Object.Resolve().Module.GetType(FullTypeName);
                if (existingType is object && existingType.IsPublic)
                    return existingType;

                return DefineAttribute(module, wellKnownTypes);
            }

            protected abstract TypeReference DefineAttribute(ModuleDefinition module, WellKnownTypes wellKnownTypes);
        }
    }
}

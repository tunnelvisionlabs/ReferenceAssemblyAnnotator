// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using Mono.Cecil;

    internal partial class WellKnownTypes
    {
        private abstract class ProvidedAttributeType : ProvidedType
        {
            protected ProvidedAttributeType(string? namespaceName, string typeName)
                : base(namespaceName, typeName)
            {
            }

            protected override sealed TypeReference DefineType(ModuleDefinition module, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
            {
                var attribute = new TypeDefinition(
                    @namespace: NamespaceName,
                    name: TypeName,
                    TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                    module.ImportReference(wellKnownTypes.SystemAttribute));

                ImplementAttribute(module, attribute, wellKnownTypes, attributeFactory);

                module.Types.Add(attribute);

                return attribute;
            }

            protected abstract void ImplementAttribute(ModuleDefinition module, TypeDefinition attribute, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory);
        }
    }
}

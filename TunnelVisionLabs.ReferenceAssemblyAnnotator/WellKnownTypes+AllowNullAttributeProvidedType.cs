// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System;
    using Mono.Cecil;

    internal partial class WellKnownTypes
    {
        private sealed class AllowNullAttributeProvidedType : ProvidedType
        {
            public AllowNullAttributeProvidedType()
                : base("System.Diagnostics.CodeAnalysis", "AllowNullAttribute")
            {
            }

            protected override TypeReference DefineAttribute(ModuleDefinition module, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
            {
                var attribute = new TypeDefinition(
                    @namespace: NamespaceName,
                    name: TypeName,
                    TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                    wellKnownTypes.Module.ImportReference(wellKnownTypes.SystemAttribute));

                attribute.AddDefaultConstructor(wellKnownTypes.TypeSystem);
                attribute.CustomAttributes.Add(attributeFactory.AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, inherited: false));

                module.Types.Add(attribute);

                return attribute;
            }
        }
    }
}

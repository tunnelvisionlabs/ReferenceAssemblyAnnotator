// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System.Linq;
    using Mono.Cecil;

    internal partial class WellKnownTypes
    {
        private sealed class EmbeddedAttributeProvidedType : ProvidedAttributeType
        {
            internal EmbeddedAttributeProvidedType()
                : base("Microsoft.CodeAnalysis", "EmbeddedAttribute")
            {
            }

            protected override void ImplementAttribute(ModuleDefinition module, TypeDefinition attribute, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
            {
                var constructor = attribute.AddDefaultConstructor(wellKnownTypes.TypeSystem);

                attribute.CustomAttributes.Add(attributeFactory.CompilerGenerated());
                attribute.CustomAttributes.Add(new CustomAttribute(constructor));
            }
        }
    }
}

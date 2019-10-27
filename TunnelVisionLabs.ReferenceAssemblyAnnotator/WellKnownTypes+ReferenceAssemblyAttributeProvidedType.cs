// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Mono.Cecil;

    internal partial class WellKnownTypes
    {
        private class ReferenceAssemblyAttributeProvidedType : ProvidedAttributeType
        {
            internal ReferenceAssemblyAttributeProvidedType()
                : base(typeof(ReferenceAssemblyAttribute).Namespace, typeof(ReferenceAssemblyAttribute).Name)
            {
            }

            protected override void ImplementAttribute(ModuleDefinition module, TypeDefinition attribute, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
            {
                attribute.AddDefaultConstructor(module.TypeSystem);

                attribute.CustomAttributes.Add(attributeFactory.CompilerGenerated());
            }
        }
    }
}

// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System.Linq;
    using Mono.Cecil;

    internal partial class WellKnownTypes
    {
        private sealed class NullablePublicOnlyAttributeProvidedType : ProvidedAttributeType
        {
            internal NullablePublicOnlyAttributeProvidedType()
                : base("System.Runtime.CompilerServices", "NullablePublicOnlyAttribute")
            {
            }

            protected override void ImplementAttribute(ModuleDefinition module, TypeDefinition attribute, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
            {
                attribute.CustomAttributes.Add(attributeFactory.CompilerGenerated());
                attribute.CustomAttributes.Add(attributeFactory.Embedded());

                var constructor = MethodFactory.Constructor(wellKnownTypes.TypeSystem);
                constructor.Parameters.Add(new ParameterDefinition(wellKnownTypes.TypeSystem.Boolean));
                attribute.Methods.Add(constructor);
            }
        }
    }
}

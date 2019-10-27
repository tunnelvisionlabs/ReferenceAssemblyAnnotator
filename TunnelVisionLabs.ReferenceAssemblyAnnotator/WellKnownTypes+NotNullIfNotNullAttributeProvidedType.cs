// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System;
    using Mono.Cecil;

    internal partial class WellKnownTypes
    {
        private sealed class NotNullIfNotNullAttributeProvidedType : ProvidedAttributeType
        {
            public NotNullIfNotNullAttributeProvidedType()
                : base("System.Diagnostics.CodeAnalysis", "NotNullIfNotNullAttribute")
            {
            }

            protected override void ImplementAttribute(ModuleDefinition module, TypeDefinition attribute, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
            {
                attribute.CustomAttributes.Add(attributeFactory.NullableContext(1));
                attribute.CustomAttributes.Add(attributeFactory.Nullable(0));
                attribute.CustomAttributes.Add(attributeFactory.AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, allowMultiple: true, inherited: false));

                var constructor = MethodFactory.Constructor(wellKnownTypes.TypeSystem);
                constructor.Parameters.Add(new ParameterDefinition("parameterName", ParameterAttributes.None, wellKnownTypes.TypeSystem.String));
                attribute.Methods.Add(constructor);
            }
        }
    }
}

// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System;
    using Mono.Cecil;

    internal partial class WellKnownTypes
    {
        private sealed class NotNullWhenAttributeProvidedType : ProvidedAttributeType
        {
            public NotNullWhenAttributeProvidedType()
                : base("System.Diagnostics.CodeAnalysis", "NotNullWhenAttribute")
            {
            }

            protected override void ImplementAttribute(ModuleDefinition module, TypeDefinition attribute, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
            {
                var constructor = MethodFactory.Constructor(wellKnownTypes.TypeSystem);
                constructor.Parameters.Add(new ParameterDefinition("returnValue", ParameterAttributes.None, wellKnownTypes.TypeSystem.Boolean));
                attribute.Methods.Add(constructor);

                attribute.CustomAttributes.Add(attributeFactory.AttributeUsage(AttributeTargets.Parameter, inherited: false));
            }
        }
    }
}

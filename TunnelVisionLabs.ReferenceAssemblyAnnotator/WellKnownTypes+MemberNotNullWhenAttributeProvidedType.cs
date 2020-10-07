// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System;
    using Mono.Cecil;
    using Mono.Cecil.Rocks;

    internal partial class WellKnownTypes
    {
        private sealed class MemberNotNullWhenAttributeProvidedType : ProvidedAttributeType
        {
            public MemberNotNullWhenAttributeProvidedType()
                : base("System.Diagnostics.CodeAnalysis", "MemberNotNullWhenAttribute")
            {
            }

            protected override void ImplementAttribute(ModuleDefinition module, TypeDefinition attribute, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
            {
                var constructor1 = MethodFactory.Constructor(wellKnownTypes.TypeSystem);
                constructor1.Parameters.Add(new ParameterDefinition("parameterValue", ParameterAttributes.None, wellKnownTypes.TypeSystem.Boolean));
                constructor1.Parameters.Add(new ParameterDefinition("member", ParameterAttributes.None, wellKnownTypes.TypeSystem.String));
                attribute.Methods.Add(constructor1);

                var members = new ParameterDefinition("members", ParameterAttributes.None, wellKnownTypes.TypeSystem.String.MakeArrayType());
                members.CustomAttributes.Add(attributeFactory.ParamArray());

                var constructor2 = MethodFactory.Constructor(wellKnownTypes.TypeSystem);
                constructor2.Parameters.Add(new ParameterDefinition("parameterValue", ParameterAttributes.None, wellKnownTypes.TypeSystem.Boolean));
                constructor2.Parameters.Add(members);
                attribute.Methods.Add(constructor2);

                attribute.CustomAttributes.Add(attributeFactory.NullableContext(1));
                attribute.CustomAttributes.Add(attributeFactory.Nullable(0));
                attribute.CustomAttributes.Add(attributeFactory.AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, inherited: false, allowMultiple: true));
            }
        }
    }
}

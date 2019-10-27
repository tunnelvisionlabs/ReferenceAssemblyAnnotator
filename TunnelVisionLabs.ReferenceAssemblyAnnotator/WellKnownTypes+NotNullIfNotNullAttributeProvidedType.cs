// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System;
    using Mono.Cecil;

    internal partial class WellKnownTypes
    {
        private sealed class NotNullIfNotNullAttributeProvidedType : ProvidedType
        {
            public NotNullIfNotNullAttributeProvidedType()
                : base("System.Diagnostics.CodeAnalysis", "NotNullIfNotNullAttribute")
            {
            }

            protected override TypeReference DefineAttribute(ModuleDefinition module, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
            {
                var attribute = new TypeDefinition(
                    @namespace: NamespaceName,
                    name: TypeName,
                    TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                    wellKnownTypes.Module.ImportReference(wellKnownTypes.SystemAttribute));

                attribute.CustomAttributes.Add(attributeFactory.NullableContext(1));
                attribute.CustomAttributes.Add(attributeFactory.Nullable(0));
                attribute.CustomAttributes.Add(attributeFactory.AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, allowMultiple: true, inherited: false));

                var constructor = MethodFactory.Constructor(wellKnownTypes.TypeSystem);
                constructor.Parameters.Add(new ParameterDefinition("parameterName", ParameterAttributes.None, wellKnownTypes.TypeSystem.String));
                attribute.Methods.Add(constructor);

                module.Types.Add(attribute);

                return attribute;
            }
        }
    }
}

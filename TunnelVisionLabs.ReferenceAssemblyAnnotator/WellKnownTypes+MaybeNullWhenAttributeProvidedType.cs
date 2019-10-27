// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System;
    using Mono.Cecil;

    internal partial class WellKnownTypes
    {
        private sealed class MaybeNullWhenAttributeProvidedType : ProvidedType
        {
            public MaybeNullWhenAttributeProvidedType()
                : base("System.Diagnostics.CodeAnalysis", "MaybeNullWhenAttribute")
            {
            }

            protected override TypeReference DefineAttribute(ModuleDefinition module, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
            {
                var attribute = new TypeDefinition(
                    @namespace: NamespaceName,
                    name: TypeName,
                    TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                    wellKnownTypes.Module.ImportReference(wellKnownTypes.SystemAttribute));

                var constructor = MethodFactory.Constructor(wellKnownTypes.TypeSystem);
                constructor.Parameters.Add(new ParameterDefinition("returnValue", ParameterAttributes.None, wellKnownTypes.TypeSystem.Boolean));
                attribute.Methods.Add(constructor);

                attribute.CustomAttributes.Add(attributeFactory.AttributeUsage(AttributeTargets.Parameter, inherited: false));

                module.Types.Add(attribute);

                return attribute;
            }
        }
    }
}

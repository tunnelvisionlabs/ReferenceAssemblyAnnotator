// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System.Linq;
    using Mono.Cecil;

    internal partial class WellKnownTypes
    {
        private sealed class NullableContextAttributeProvidedType : ProvidedType
        {
            internal NullableContextAttributeProvidedType()
                : base("System.Runtime.CompilerServices", "NullableContextAttribute")
            {
            }

            protected override TypeReference DefineAttribute(ModuleDefinition module, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
            {
                var attribute = new TypeDefinition(
                    @namespace: NamespaceName,
                    name: TypeName,
                    TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                    wellKnownTypes.Module.ImportReference(wellKnownTypes.SystemAttribute));

                MethodDefinition compilerGeneratedConstructor = wellKnownTypes.SystemRuntimeCompilerServicesCompilerGeneratedAttribute.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 0);
                attribute.CustomAttributes.Add(new CustomAttribute(wellKnownTypes.Module.ImportReference(compilerGeneratedConstructor)));
                attribute.CustomAttributes.Add(new CustomAttribute(wellKnownTypes.MicrosoftCodeAnalysisEmbeddedAttribute.Value.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 0)));

                var constructor = MethodFactory.Constructor(wellKnownTypes.TypeSystem);
                constructor.Parameters.Add(new ParameterDefinition(wellKnownTypes.TypeSystem.Byte));
                attribute.Methods.Add(constructor);

                module.Types.Add(attribute);

                return attribute;
            }
        }
    }
}

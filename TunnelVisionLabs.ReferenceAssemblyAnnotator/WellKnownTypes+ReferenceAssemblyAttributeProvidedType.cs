// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Mono.Cecil;

    internal partial class WellKnownTypes
    {
        private class ReferenceAssemblyAttributeProvidedType : ProvidedType
        {
            internal ReferenceAssemblyAttributeProvidedType()
                : base(typeof(ReferenceAssemblyAttribute).Namespace, typeof(ReferenceAssemblyAttribute).Name)
            {
            }

            protected override TypeReference DefineAttribute(ModuleDefinition module, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
            {
                var attribute = new TypeDefinition(
                    @namespace: NamespaceName,
                    name: TypeName,
                    TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                    module.ImportReference(wellKnownTypes.SystemAttribute));

                attribute.AddDefaultConstructor(module.TypeSystem);

                MethodDefinition compilerGeneratedConstructor = wellKnownTypes.SystemRuntimeCompilerServicesCompilerGeneratedAttribute.Resolve().Methods.Single(method => method.IsConstructor && !method.IsStatic && method.Parameters.Count == 0);
                var customAttribute = new CustomAttribute(module.ImportReference(compilerGeneratedConstructor));
                attribute.CustomAttributes.Add(customAttribute);

                module.Types.Add(attribute);

                return attribute;
            }
        }
    }
}

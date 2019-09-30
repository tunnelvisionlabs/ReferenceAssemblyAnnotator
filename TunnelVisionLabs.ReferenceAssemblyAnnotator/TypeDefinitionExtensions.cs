// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using Mono.Cecil;

    internal static class TypeDefinitionExtensions
    {
        public static MethodDefinition AddDefaultConstructor(this TypeDefinition typeDefinition, TypeSystem typeSystem)
        {
            MethodDefinition constructor = MethodFactory.DefaultConstructor(typeSystem);
            typeDefinition.Methods.Add(constructor);
            return constructor;
        }
    }
}

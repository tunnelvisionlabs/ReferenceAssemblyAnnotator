// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CecilBasedAnnotator
{
    internal static class TypeDefinitionExtensions
    {
        public static MethodDefinition AddDefaultConstructor(this TypeDefinition typeDefinition, WellKnownTypes wellKnownTypes)
        {
            MethodDefinition constructor = MethodFactory.DefaultConstructor(wellKnownTypes);
            typeDefinition.Methods.Add(constructor);
            return constructor;
        }
    }
}

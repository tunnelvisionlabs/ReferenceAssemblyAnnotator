// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    internal static class MethodFactory
    {
        public static MethodDefinition DefaultConstructor(WellKnownTypes wellKnownTypes)
            => Constructor(wellKnownTypes);

        public static MethodDefinition Constructor(WellKnownTypes wellKnownTypes)
        {
            var constructor = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, wellKnownTypes.TypeSystem.Void);
            constructor.Body = new MethodBody(constructor)
            {
                Instructions =
                {
                    Instruction.Create(OpCodes.Ldnull),
                    Instruction.Create(OpCodes.Throw),
                },
            };

            return constructor;
        }
    }
}

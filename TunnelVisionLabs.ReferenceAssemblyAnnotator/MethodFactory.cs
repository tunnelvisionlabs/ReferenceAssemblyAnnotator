// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    internal static class MethodFactory
    {
        public static MethodDefinition DefaultConstructor(TypeSystem typeSystem)
            => Constructor(typeSystem);

        public static MethodDefinition Constructor(TypeSystem typeSystem)
        {
            var constructor = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, typeSystem.Void);
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

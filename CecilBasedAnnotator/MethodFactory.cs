using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CecilBasedAnnotator
{
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

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

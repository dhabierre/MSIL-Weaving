namespace Mono.Cecil.Cil
{
    using Collections.Generic;

    public static class InstructionExtensions
    {
        public static Collection<ParameterDefinition> GetParameters(this Instruction instruction)
            => ((GenericInstanceMethod)instruction.Operand).Parameters;

        public static Collection<TypeReference> GetGenericArguments(this Instruction instruction)
            => ((GenericInstanceMethod)instruction.Operand).GenericArguments;
    }
}

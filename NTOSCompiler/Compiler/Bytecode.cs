namespace NTOSCompiler;

public enum OpCode
{
    PushInt,
    PushFloat,
    PushString,

    LoadVariable,
    StoreVariable,

    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,

    CallFunction,

    Pop
}

public readonly record struct Instruction(
    OpCode OpCode,
    object? Operand = null);

public sealed class BytecodeProgram
{
    public List<Instruction> Instructions { get; } = new();
}

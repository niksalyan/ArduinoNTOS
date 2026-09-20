namespace NTOSCompiler;

public enum OpCode
{
    PushInt,
    PushFloat,
    PushString,

    PushBool,
    LoadVariable,
    StoreVariable,

    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,

    CallFunction,

    Pop,
    Equal,
    NotEqual,
}

public readonly record struct Instruction(
    OpCode OpCode,
    object? Operand = null);

public sealed class BytecodeProgram
{
    public List<Instruction> Instructions { get; } = new();
}

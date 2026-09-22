namespace NTOSCompiler;

public enum OpCode : byte
{
    End = 0,
    PushInt = 1,
    PushFloat,
    PushString,

    PushBool,
    LoadVariable,
    StoreVariable,

    Add = 32,
    Subtract,
    Multiply,
    Divide,
    Modulo,

    

    Pop,
    Equal,
    NotEqual,

    Less,
    Greater,
    LessEqual,
    GreaterEqual,

    And,
    Or,
    Not,

    Jump = 128,
    JumpIfFalse,
    CallFunction,
}

public readonly record struct Instruction(
    OpCode OpCode,
    object? Operand = null);

public sealed class BytecodeProgram
{
    public List<Instruction> Instructions { get; } = new();
}

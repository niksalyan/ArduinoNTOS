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
    public List<KeyValuePair<string, int>> _addresses = new();

    public List<Instruction> Instructions { get; } = new();

    private int _nextAddress;

    public int GetAddress(string name, int size)
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(size));

        foreach (var entry in _addresses)
        {
            if (entry.Key == name)
                return entry.Value;
        }

        int address = _nextAddress;

        _addresses.Add(
            new KeyValuePair<string, int>(
                name,
                address));

        _nextAddress += size;

        return address;
    }
}

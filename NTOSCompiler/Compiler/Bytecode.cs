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

    public byte[] ToByteCode()
    {
        using var stream = new MemoryStream();

        foreach (var instruction in Instructions)
        {
            stream.WriteByte((byte)instruction.OpCode);

            switch (instruction.OpCode)
            {
                case OpCode.PushInt:
                    WriteInt32(
                        stream,
                        Convert.ToInt32(instruction.Operand));
                    break;

                case OpCode.PushFloat:
                    WriteFloat(
                        stream,
                        Convert.ToSingle(instruction.Operand));
                    break;

                case OpCode.PushBool:
                    stream.WriteByte(
                        Convert.ToBoolean(instruction.Operand)
                            ? (byte)1
                            : (byte)0);
                    break;

                case OpCode.LoadVariable:
                case OpCode.StoreVariable:
                    WriteAddress(
                        stream,
                        Convert.ToInt32(instruction.Operand));
                    break;

                case OpCode.Jump:
                case OpCode.JumpIfFalse:
                    WriteInt32(
                        stream,
                        Convert.ToInt32(instruction.Operand));
                    break;

                case OpCode.Add:
                case OpCode.Subtract:
                case OpCode.Multiply:
                case OpCode.Divide:
                case OpCode.Modulo:
                case OpCode.Pop:
                case OpCode.Equal:
                case OpCode.NotEqual:
                case OpCode.Less:
                case OpCode.Greater:
                case OpCode.LessEqual:
                case OpCode.GreaterEqual:
                case OpCode.And:
                case OpCode.Or:
                case OpCode.Not:
                case OpCode.End:
                    break;

                case OpCode.PushString:
                    throw new NotSupportedException(
                        "String bytecode is not implemented yet.");

                case OpCode.CallFunction:
                    throw new NotSupportedException(
                        "Function bytecode is not implemented yet.");

                default:
                    throw new InvalidOperationException(
                        $"Unsupported opcode: {instruction.OpCode}");
            }
        }

        return stream.ToArray();
    }

    private static void WriteInt32(
        Stream stream,
        int value)
    {
        stream.Write(
            BitConverter.GetBytes(value));
    }

    private static void WriteFloat(
        Stream stream,
        float value)
    {
        stream.Write(
            BitConverter.GetBytes(value));
    }

    private static void WriteAddress(
        Stream stream,
        int address)
    {
        stream.Write(
            BitConverter.GetBytes(
                checked((ushort)address)));
    }

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

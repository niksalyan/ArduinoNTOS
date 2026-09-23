namespace NTOSCompiler;

public enum OpCode : byte
{
    End = 0,
    PushInt = 1,
    PushFloat,
    PushStr,

    PushBool,


    LoadInt,
    StoreInt,

    LoadFloat,
    StoreFloat,

    LoadBool,
    StoreBool,

    LoadStr,
    StoreStr,

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

public class Instruction
{
    public OpCode OpCode { get; }
    public object? Operand { get; }
    public int Address { get; set; }

    public Instruction(
        OpCode opCode,
        object? operand = null)
    {
        OpCode = opCode;
        Operand = operand;
    }
}

public enum VariableType { 
    Void, // Just in case, we can remove this if not needed
    Int,
    Float,
    Bool,
    Str
}

public class Variable
{
    public string Name { get; }
    public VariableType Type { get; }
    public int Address { get; set; }

    public bool IsArray { get; }
    public int Length { get; }

    public int StringLength { get; }

    public Variable(
        string name,
        VariableType type,
        bool isArray = false,
        int length = 1,
        int stringLength = 0
        )
    {
        Name = name;
        Type = type;
        IsArray = isArray;
        Length = length;
        StringLength = stringLength;
    }

    public int GetSize()
    {
        switch(Type)
        {
            case VariableType.Int:
                return 4 * Length;
            case VariableType.Float:
                return 4 * Length;
            case VariableType.Bool:
                return 1 * Length;
            case VariableType.Str:
                return (StringLength + 1) * Length; // Probably string size + ending zero
            default:
                return 0;
        }
    }
}

public sealed class BytecodeProgram
{
    private List<Variable> _variables = new();

    public List<Instruction> Instructions { get; } = new();

    private byte[] _bytecode;

    public byte[] Bytecode => _bytecode;

    public void UpdateBytecode()
    {
        PrepareBytecode(true);
        _bytecode = PrepareBytecode();
    }

    public byte[] PrepareBytecode(
    bool assignAddresses = false)
    {
        using var stream = new MemoryStream();

        foreach (var instruction in Instructions)
        {
            if (assignAddresses)
            {
                instruction.Address =
                    checked((int)stream.Length);
            }

            stream.WriteByte(
                (byte)instruction.OpCode);

            switch (instruction.OpCode)
            {
                case OpCode.PushInt:
                    WriteInt32(
                        stream,
                        Convert.ToInt32(
                            instruction.Operand));
                    break;

                case OpCode.PushFloat:
                    WriteFloat(
                        stream,
                        Convert.ToSingle(
                            instruction.Operand));
                    break;

                case OpCode.PushBool:
                    stream.WriteByte(
                        Convert.ToBoolean(
                            instruction.Operand)
                            ? (byte)1
                            : (byte)0);
                    break;

                case OpCode.LoadInt:
                case OpCode.StoreInt:
                case OpCode.LoadFloat:
                case OpCode.StoreFloat:
                case OpCode.LoadBool:
                case OpCode.StoreBool:
                case OpCode.LoadStr:
                case OpCode.StoreStr:
                    WriteAddress(
                        stream,
                        Convert.ToInt32(
                            instruction.Operand));
                    break;

                case OpCode.Jump:
                case OpCode.JumpIfFalse:
                    {
                        int targetInstruction =
                            Convert.ToInt32(
                                instruction.Operand);

                        int targetAddress =
                            targetInstruction < Instructions.Count
                                ? Instructions[targetInstruction].Address
                                : (int)stream.Length + 4;

                        WriteInt32(
                            stream,
                            targetAddress);

                        break;
                    }

                case OpCode.CallFunction:
                    {
                        var call =
                            (FunctionCall)instruction.Operand!;

                        stream.WriteByte(
                            (byte)call.Index);

                        stream.WriteByte(
                            (byte)(call.Index >> 8));

                        stream.WriteByte(
                            call.ArgumentCount);

                        break;
                    }

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

                case OpCode.PushStr:
                    throw new NotSupportedException(
                        "String bytecode is not implemented yet.");

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

    public Variable GetVariable(string name)
    {
        var variable = _variables.FirstOrDefault(x => x.Name == name);

        if (variable == null)
            throw new InvalidOperationException(
                $"Variable '{name}' is not declared.");

        return variable;
    }

    public Variable DeclareVariable(
    string name,
    VariableType type,
    bool isArray = false,
    int length = 1,
    int stringLength = 0)
    {
        if (_variables.Any(x => x.Name == name))
            throw new InvalidOperationException(
                $"Variable '{name}' is already declared.");

        var variable = new Variable(
            name,
            type,
            isArray,
            length,
            stringLength);

        _variables.Add(variable);
        UpdateAddresses();

        return variable;
    }

    private void UpdateAddresses()
    {
        int address = 0;
        foreach (var variable in _variables)
        {
            variable.Address = address;
            address += variable.GetSize();
        }
    }
}

using NTOSCompiler.Compiler;
using System.Buffers.Binary;

namespace NTOSCompiler;

public enum OpCode : byte
{
    Exit = 0,
    PushInt = 1,
    PushFloat,
    PushString,
    PushBool,

    LoadInt,
    StoreInt,

    LoadFloat,
    StoreFloat,

    Add = 32,
    Subtract,
    Multiply,
    Divide,
    Modulo,

    CallFunction = 64,
    CallUserFunction,

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
    Return
}

public readonly record struct Instruction(
    OpCode OpCode,
    object? Operand = null);



public enum VariableType
{
    Void = 0,
    Int,
    Float,
    Bool,
    String
}

public sealed class VariableDefinition
{
    public int Address { get; set; }
    public string Name { get; }
    public VariableType Type { get; }

    public VariableDefinition(
        string name,
        VariableType type)
    {
        Name = name;
        Type = type;
    }

    public override string ToString()
        => $"{Name} ({Type})";
}


public sealed class BytecodeProgram
{
    public List<Instruction> Instructions { get; } = new();

    public List<VariableDefinition> Variables { get; } = new();

    public List<FunctionDefinition> Functions { get; } = new();

    public byte[] ToByteCode()
    {
        using var stream = new MemoryStream();

        foreach (var instruction in Instructions)
        {
            stream.WriteByte((byte)instruction.OpCode);

            switch (instruction.OpCode)
            {
                case OpCode.PushInt:
                    WriteInt32(stream, Convert.ToInt32(instruction.Operand));
                    break;

                case OpCode.PushFloat:
                    WriteFloat(stream, Convert.ToSingle(instruction.Operand));
                    break;

                case OpCode.PushBool:
                    stream.WriteByte(
                        Convert.ToBoolean(instruction.Operand)
                            ? (byte)1
                            : (byte)0);
                    break;

                case OpCode.PushString:
                    throw new NotImplementedException();

                case OpCode.LoadInt:
                case OpCode.StoreInt:
                case OpCode.LoadFloat:
                case OpCode.StoreFloat:
                case OpCode.Jump:
                case OpCode.JumpIfFalse:
                    WriteInt32(stream, Convert.ToInt32(instruction.Operand));
                    break;

                case OpCode.CallFunction:
                    {
                        var call = (FunctionCall)instruction.Operand!;

                        stream.Write(BitConverter.GetBytes(call.FunctionIndex));
                        stream.Write(BitConverter.GetBytes(call.ArgumentCount));

                        break;
                    }

                case OpCode.CallUserFunction:
                    {
                        var call = (UserFunctionCall)instruction.Operand!;

                        stream.Write(BitConverter.GetBytes(call.EntryPoint));
                        stream.Write(BitConverter.GetBytes(call.ParameterAddresses.Length));

                        foreach (var address in call.ParameterAddresses)
                        {
                            stream.Write(BitConverter.GetBytes(address));
                        }

                        break;
                    }

                case OpCode.Add:
                case OpCode.Subtract:
                case OpCode.Multiply:
                case OpCode.Divide:
                case OpCode.Modulo:
                case OpCode.Exit:
                case OpCode.Pop:
                case OpCode.Return:
                case OpCode.Equal:
                case OpCode.NotEqual:
                case OpCode.Less:
                case OpCode.Greater:
                case OpCode.LessEqual:
                case OpCode.GreaterEqual:
                case OpCode.And:
                case OpCode.Or:
                case OpCode.Not:
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported opcode: {instruction.OpCode}");
            }
        }

        return stream.ToArray();
    }

    private static void WriteInt32(Stream stream, int value)
    {
        Span<byte> buffer = stackalloc byte[4];

        BinaryPrimitives.WriteInt32LittleEndian(buffer, value);

        stream.Write(buffer);
    }

    private static void WriteFloat(Stream stream, float value)
    {
        WriteInt32(
            stream,
            BitConverter.SingleToInt32Bits(value));
    }

    public VariableDefinition GetVariable(
        string variableName,
        string functionName,
        VariableType type
        )
    {

        var scopedName = (functionName != null ? functionName + ":" : "") + variableName;

        var existingScoped = Variables.FirstOrDefault(x => x.Name == scopedName);

        if (existingScoped != null)
        {
            return existingScoped;
        }

        if (functionName != null) {
            var existingGlobal = Variables.FirstOrDefault(x => x.Name == variableName);
            if (existingGlobal != null)
            {
                return existingGlobal;
            }
        }

        if (type == VariableType.Void)
        {
            throw new InvalidOperationException(
                $"Unknown variable '{variableName}'.");
        }

        var variable = new VariableDefinition(
            scopedName,
            type);

        
        Variables.Add(variable);
        UpdateAddresses();
        return variable;
    }

    private void UpdateAddresses()
    {
        int address = 0;
        foreach (var variable in Variables)
        {
            variable.Address = address;
            address += variable.Type switch
            {
                VariableType.Int => 4,
                VariableType.Float => 4,
                VariableType.Bool => 1,
                VariableType.String => 32,
                _ => throw new Exception($"Unknown variable type: {variable.Type}")
            };
        }
    }

}

using NTOSCompiler.Compiler;
using System.ComponentModel;

namespace NTOSCompiler;

public enum OpCode : byte
{
    End = 0,
    PushInt = 1,
    PushFloat,
    PushStr,

    PushByte,
    

    LoadInt,
    StoreInt,

    LoadFloat,
    StoreFloat,

    LoadByte,
    StoreByte,

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

    Checkpoint = 255
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
    None, // Just in case, we can remove this if not needed
    Int,
    Float,

    Byte,
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

    public int MaxLength { get; }

    public Variable(
        string name,
        VariableType type,
        bool isArray = false,
        int length = 1,
        int maxLength = 0
        )
    {
        Name = name;
        Type = type;
        IsArray = isArray;
        Length = length;
        MaxLength = maxLength;
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
                return (MaxLength + 1) * Length; // Probably string size + ending zero
            default:
                return 0;
        }
    }
}

public class BytecodeApp
{
    private List<Variable> _variables = new();
    private List<Instruction> _instructions;

    public List<Variable> Variables => _variables;
    public List<Instruction> Instructions => _instructions;
    private Dictionary<string, byte[]> _bytecodes = new();
    private VMFunctions _vmFunctions;

    public BytecodeApp(VMFunctions vmFunctions)
    {
        _vmFunctions = vmFunctions;
    }

    public byte[] GetBytecode(string name)
    {
        return _bytecodes.ContainsKey(name) ? _bytecodes[name] : new byte[1];
    }

    public string? CompileSource(string name, string source)
    {
        try
        {
            _instructions = null;
            var lexer = new Lexer(source);
            var tokens = lexer.Tokenize();

            var bytecode = new BytecodeProgram(_variables);

            var parser = new Parser(tokens, _vmFunctions);
            var program = parser.Compile(bytecode);

            _bytecodes[name] = program.Bytecode;
            _instructions = program.Instructions;

            return null;
        } catch(Exception ex)
        {
            return ex.Message;
        }
    }

    public void Reset()
    {
        _variables.Clear();
        _bytecodes.Clear();
    }
}

public class BytecodeProgram
{
    private List<Variable> _variables;

    public List<Instruction> Instructions { get; } = new();

    private byte[] _bytecode;

    public byte[] Bytecode => _bytecode;

    public BytecodeProgram(List<Variable> sharedVariables = null)
    {
        _variables = sharedVariables ?? new();
    }

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
                case OpCode.PushByte:
                    stream.WriteByte(
                        Convert.ToByte(instruction.Operand));
                    break;

                case OpCode.LoadInt:
                case OpCode.StoreInt:
                case OpCode.LoadFloat:
                case OpCode.StoreFloat:
                case OpCode.LoadByte:
                case OpCode.StoreByte:
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
                case OpCode.Checkpoint:
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
    int maxLength = 0)
    {
        var variable = _variables.FirstOrDefault(x => x.Name == name);
        if (variable != null)
        {
            return variable;
        }
        
        variable = new Variable(
            name,
            type,
            isArray,
            length,
            maxLength);

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

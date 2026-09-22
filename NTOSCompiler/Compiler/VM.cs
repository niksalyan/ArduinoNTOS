using NTOSCompiler.Compiler;

namespace NTOSCompiler;

public sealed class VirtualMachine
{
    private readonly Dictionary<string, object?> _variables = new();
    private readonly Stack<object?> _stack = new();

    private readonly VMFunctions _vmFunctions;

    private readonly byte[] _memory;


    public VirtualMachine(int memorySize = 4096, VMFunctions vmFunctions = null)
    {
        _vmFunctions = vmFunctions;
        _memory = new byte[memorySize];
    }

    public object? Execute(byte[] bytecode)
    {
        _stack.Clear();

        int instructionPointer = 0;
        while (instructionPointer < bytecode.Length)
        {
            OpCode opcode =
                (OpCode)bytecode[instructionPointer++];
            switch (opcode)
            {
                case OpCode.PushInt:
                    {
                        int value =
                            ReadInt32(
                                bytecode,
                                ref instructionPointer);

                        _stack.Push(value);
                        break;
                    }

                case OpCode.PushFloat:
                    {
                        float value =
                            ReadFloat(
                                bytecode,
                                ref instructionPointer);

                        _stack.Push(value);
                        break;
                    }

                case OpCode.PushBool:
                    {
                        bool value =
                            bytecode[instructionPointer++] != 0;

                        _stack.Push(value);
                        break;
                    }
                case OpCode.PushString:
                    // _stack.Push(instruction.Operand);
                    break;



                case OpCode.LoadVariable:
                    {
                        ushort address =
                            ReadAddress(
                                bytecode,
                                ref instructionPointer);

                        _stack.Push(
                            GetInt(address));

                        break;
                    }

                case OpCode.StoreVariable:
                    {
                        ushort address =
                            ReadAddress(
                                bytecode,
                                ref instructionPointer);

                        int value =
                            Convert.ToInt32(
                                _stack.Pop());

                        SetInt(address, value);

                        break;
                    }

                case OpCode.Add:
                    BinaryNumeric((a, b) => a + b);
                    break;

                case OpCode.Subtract:
                    BinaryNumeric((a, b) => a - b);
                    break;

                case OpCode.Multiply:
                    BinaryNumeric((a, b) => a * b);
                    break;

                case OpCode.Divide:
                    BinaryNumeric((a, b) => a / b);
                    break;

                case OpCode.Modulo:
                    BinaryNumeric((a, b) => a % b);
                    break;

                case OpCode.Equal:
                    {
                        object right = _stack.Pop()!;
                        object left = _stack.Pop()!;

                        _stack.Push(Equals(left, right));
                        break;
                    }

                case OpCode.NotEqual:
                    {
                        object right = _stack.Pop()!;
                        object left = _stack.Pop()!;

                        _stack.Push(!Equals(left, right));
                        break;
                    }

                case OpCode.Less:
                    Compare((a, b) => a < b);
                    break;

                case OpCode.Greater:
                    Compare((a, b) => a > b);
                    break;

                case OpCode.LessEqual:
                    Compare((a, b) => a <= b);
                    break;

                case OpCode.GreaterEqual:
                    Compare((a, b) => a >= b);
                    break;

                case OpCode.And:
                    {
                        bool right = Convert.ToBoolean(_stack.Pop());
                        bool left = Convert.ToBoolean(_stack.Pop());

                        _stack.Push(left && right);
                        break;
                    }

                case OpCode.Or:
                    {
                        bool right = Convert.ToBoolean(_stack.Pop());
                        bool left = Convert.ToBoolean(_stack.Pop());

                        _stack.Push(left || right);
                        break;
                    }

                case OpCode.Not:
                    {
                        bool value = Convert.ToBoolean(_stack.Pop());

                        _stack.Push(!value);
                        break;
                    }

                case OpCode.Jump:
                    {
                        instructionPointer =
        ReadInt32(bytecode, ref instructionPointer);

                        continue;
                    }

                case OpCode.JumpIfFalse:
                    {
                        bool condition =
                            Convert.ToBoolean(_stack.Pop());

                        int target =
                            ReadInt32(bytecode, ref instructionPointer);

                        if (!condition)
                        {
                            instructionPointer = target;
                            continue;
                        }

                        break;
                    }

                case OpCode.CallFunction:
                    {
                        /*var call = (FunctionCall)instruction.Operand!;

                        var args =
                            new object?[call.ArgumentCount];

                        for (int i = call.ArgumentCount - 1; i >= 0; i--)
                            args[i] = _stack.Pop();


                        object? result = _vmFunctions.Invoke(call.Name, args); //  Invoke by index here

                        _stack.Push(result);*/
                        break;
                    }

                case OpCode.Pop:
                    _stack.Pop();
                    break;

                default:
                    throw new Exception(
                        $"Unsupported opcode: {opcode}");
            }
        }




        return _stack.Count > 0
            ? _stack.Peek()
            : null;
    }

    private int GetInt(ushort address)
    {
        if (address + 4 > _memory.Length)
            throw new Exception(
                $"Memory access out of range: {address}");

        return BitConverter.ToInt32(_memory, address);
    }

    private void SetInt(ushort address, int value)
    {
        if (address + 4 > _memory.Length)
            throw new Exception(
                $"Memory access out of range: {address}");

        byte[] bytes = BitConverter.GetBytes(value);

        Buffer.BlockCopy(
            bytes,
            0,
            _memory,
            address,
            4);
    }

    private void Compare(
        Func<dynamic, dynamic, bool> operation)
    {
        double right =
            Convert.ToDouble(_stack.Pop());

        double left =
            Convert.ToDouble(_stack.Pop());

        _stack.Push(
            operation(left, right));
    }

    private static int ReadInt32(
    byte[] bytecode,
    ref int pc)
    {
        int value =
            BitConverter.ToInt32(
                bytecode,
                pc);

        pc += 4;

        return value;
    }

    private static float ReadFloat(
        byte[] bytecode,
        ref int pc)
    {
        float value =
            BitConverter.ToSingle(
                bytecode,
                pc);

        pc += 4;

        return value;
    }

    private static ushort ReadAddress(
        byte[] bytecode,
        ref int pc)
    {
        ushort value =
            BitConverter.ToUInt16(
                bytecode,
                pc);

        pc += 2;

        return value;
    }

    public object? GetVariable(string name)
        => _variables.TryGetValue(
            name,
            out var value)
                ? value
                : null;

    private void BinaryNumeric(
        Func<dynamic, dynamic, dynamic> operation)
    {
        double right =
            Convert.ToDouble(_stack.Pop());

        double left =
            Convert.ToDouble(_stack.Pop());

        _stack.Push(
            operation(left, right));
    }
}

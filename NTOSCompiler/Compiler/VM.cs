using NTOSCompiler.Compiler;
using System.Buffers.Binary;

namespace NTOSCompiler;

public sealed class VirtualMachine
{

    private readonly Stack<object?> _stack = new();
    private readonly Stack<int> _returnStack = new();

    private readonly byte[] _variableMemory;


    private VMFunctions _vmFunctions;

    public VirtualMachine(int variableMemorySize = 4096, VMFunctions vmFunctions = null)
    {
        _vmFunctions = vmFunctions;
        _variableMemory = new byte[variableMemorySize];
    }

    public object? Execute(byte[] program)
    {
        _stack.Clear();
        _returnStack.Clear();

        int pc = 0;

        while (pc < program.Length)
        {
            var opcode = (OpCode)program[pc++];

            switch (opcode)
            {
                case OpCode.PushInt:
                    int valueInt = ReadInt32(program, ref pc);
                    _stack.Push(valueInt);
                    break;
                case OpCode.PushFloat:
                    float valueFloat = ReadFloat(program, ref pc);
                    _stack.Push(valueFloat);
                    break;
                case OpCode.PushString:
                    //_stack.Push(instruction.Operand);
                    break;

                case OpCode.PushBool:
                    //_stack.Push(instruction.Operand);
                    break;

                case OpCode.LoadInt:
                    _stack.Push(GetInt(ReadInt32(program, ref pc)));
                    break;
                case OpCode.StoreInt:
                    SetInt(ReadInt32(program, ref pc), Convert.ToInt32(_stack.Pop()));
                    break;
                case OpCode.LoadFloat:
                    _stack.Push(GetFloat(ReadInt32(program, ref pc)));
                    break;
                case OpCode.StoreFloat:
                    SetFloat(ReadInt32(program, ref pc), Convert.ToSingle(_stack.Pop()));
                    break;
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
                        /*instructionPointer =
                            Convert.ToInt32(instruction.Operand);*/

                        continue;
                    }

                case OpCode.JumpIfFalse:
                    {
                        bool condition =
                            Convert.ToBoolean(_stack.Pop());

                        if (!condition)
                        {
                            /*instructionPointer =
                                Convert.ToInt32(instruction.Operand);*/

                            continue;
                        }

                        break;
                    }
                case OpCode.CallUserFunction:

                    int entryPoint = ReadInt32(program, ref pc);
                    int argumentCount2 = ReadInt32(program, ref pc);

                    _returnStack.Push(pc);

                    for (int i = argumentCount2 - 1; i >= 0; i--)
                    {
                        int address = ReadInt32(program, ref pc);
                        int value = Convert.ToInt32(_stack.Pop());

                        SetInt(address, value);
                    }

                    pc = entryPoint;
                    break;

                case OpCode.CallFunction:
                    int entryPoint2 = ReadInt32(program, ref pc);
                    int argumentCount = ReadInt32(program, ref pc);

                    var parameterAddresses = new int[argumentCount];

                    for (int i = 0; i < argumentCount; i++)
                    {
                        parameterAddresses[i] =
                            ReadInt32(program, ref pc);
                    }

                    var args = new object?[argumentCount];

                    for (int i = argumentCount - 1; i >= 0; i--)
                    {
                        args[i] = _stack.Pop();
                    }

                    for (int i = 0; i < argumentCount; i++)
                    {
                        SetInt(
                            parameterAddresses[i],
                            Convert.ToInt32(args[i]));
                    }

                    _returnStack.Push(pc);

                    pc = entryPoint2;

                    break;
                case OpCode.Return:
                    {
                        if (_returnStack.Count == 0)
                            return _stack.Count > 0
                                ? _stack.Peek()
                                : null;

                        pc = _returnStack.Pop();
                        break;
                    }
                case OpCode.Pop:
                    _stack.Pop();
                    break;
                case OpCode.Exit:
                    return null;

                default:
                    throw new Exception(
                        $"Unsupported opcode: {opcode}");
            }
        }

        return _stack.Count > 0
            ? _stack.Peek()
            : null;
    }

    private void SetInt(int address, int value)
    {
        BitConverter.GetBytes(value)
            .CopyTo(_variableMemory, address);
    }

    private int GetInt(int address)
    {
        return BitConverter.ToInt32(_variableMemory, address);
    }

    private void SetFloat(int address, float value)
    {
        BitConverter.GetBytes(value)
            .CopyTo(_variableMemory, address);
    }

    private float GetFloat(int address)
    {
        return BitConverter.ToSingle(_variableMemory, address);
    }

    private void Compare(
    Func<dynamic, dynamic, bool> operation)
    {
        dynamic right = _stack.Pop()!;
        dynamic left = _stack.Pop()!;

        _stack.Push(
            operation(left, right));
    }

    private static int ReadInt32(byte[] program, ref int pc)
    {
        int value = BinaryPrimitives.ReadInt32LittleEndian(
            program.AsSpan(pc, 4));

        pc += 4;

        return value;
    }

    private static float ReadFloat(byte[] program, ref int pc)
    {
        int bits = ReadInt32(program, ref pc);

        return BitConverter.Int32BitsToSingle(bits);
    }


    private void BinaryNumeric(
    Func<dynamic, dynamic, dynamic> operation)
    {
        dynamic right = _stack.Pop()!;
        dynamic left = _stack.Pop()!;

        _stack.Push(operation(left, right));
    }
}

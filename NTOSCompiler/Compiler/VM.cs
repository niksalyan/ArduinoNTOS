using NTOSCompiler.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NTOSCompiler;

public sealed class VirtualMachine
{
    private readonly Stack<object?> _stack = new();

    private readonly VMFunctions _vmFunctions;

    private readonly byte[] _memory;

    public uint MaxInstructions { get; set; } = 100_000;

    public VirtualMachine(
        int memorySize = 4096,
        VMFunctions vmFunctions = null)
    {
        _vmFunctions = vmFunctions;
        _memory = new byte[memorySize];
    }

    public void Execute(byte[] bytecode)
    {
        _stack.Clear();

        int instructionPointer = 0;
        uint instructionCount = 0;

        while (instructionPointer < bytecode.Length)
        {
            if (++instructionCount > MaxInstructions)
            {
                throw new Exception(
                    "VM execution limit exceeded. Possible infinite loop.");
            }

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

                case OpCode.PushStr:
                    throw new NotSupportedException(
                        "String values are not implemented yet.");

                case OpCode.LoadInt:
                    {
                        ushort address =
                            ReadAddress(
                                bytecode,
                                ref instructionPointer);

                        _stack.Push(
                            GetInt(address));

                        break;
                    }

                case OpCode.StoreInt:
                    {
                        ushort address =
                            ReadAddress(
                                bytecode,
                                ref instructionPointer);

                        int value =
                            Convert.ToInt32(_stack.Pop());

                        SetInt(address, value);

                        break;
                    }

                case OpCode.LoadFloat:
                    {
                        ushort address =
                            ReadAddress(
                                bytecode,
                                ref instructionPointer);

                        _stack.Push(
                            GetFloat(address));

                        break;
                    }

                case OpCode.StoreFloat:
                    {
                        ushort address =
                            ReadAddress(
                                bytecode,
                                ref instructionPointer);

                        float value =
                            Convert.ToSingle(_stack.Pop());

                        SetFloat(address, value);

                        break;
                    }

                case OpCode.LoadBool:
                    {
                        ushort address =
                            ReadAddress(
                                bytecode,
                                ref instructionPointer);

                        _stack.Push(
                            GetBool(address));

                        break;
                    }

                case OpCode.StoreBool:
                    {
                        ushort address =
                            ReadAddress(
                                bytecode,
                                ref instructionPointer);

                        bool value =
                            Convert.ToBoolean(_stack.Pop());

                        SetBool(address, value);

                        break;
                    }

                case OpCode.LoadStr:
                    throw new NotSupportedException(
                        "String variables are not implemented yet.");

                case OpCode.StoreStr:
                    throw new NotSupportedException(
                        "String variables are not implemented yet.");

                case OpCode.Add:
                    BinaryNumeric(
                        (a, b) => a + b);

                    break;

                case OpCode.Subtract:
                    BinaryNumeric(
                        (a, b) => a - b);

                    break;

                case OpCode.Multiply:
                    BinaryNumeric(
                        (a, b) => a * b);

                    break;

                case OpCode.Divide:
                    BinaryNumeric(
                        (a, b) => a / b);

                    break;

                case OpCode.Modulo:
                    BinaryNumeric(
                        (a, b) => a % b);

                    break;

                case OpCode.Equal:
                    {
                        object right =
                            _stack.Pop()!;

                        object left =
                            _stack.Pop()!;

                        _stack.Push(
                            Equals(left, right));

                        break;
                    }

                case OpCode.NotEqual:
                    {
                        object right =
                            _stack.Pop()!;

                        object left =
                            _stack.Pop()!;

                        _stack.Push(
                            !Equals(left, right));

                        break;
                    }

                case OpCode.Less:
                    Compare(
                        (a, b) => a < b);

                    break;

                case OpCode.Greater:
                    Compare(
                        (a, b) => a > b);

                    break;

                case OpCode.LessEqual:
                    Compare(
                        (a, b) => a <= b);

                    break;

                case OpCode.GreaterEqual:
                    Compare(
                        (a, b) => a >= b);

                    break;

                case OpCode.And:
                    {
                        bool right =
                            Convert.ToBoolean(
                                _stack.Pop());

                        bool left =
                            Convert.ToBoolean(
                                _stack.Pop());

                        _stack.Push(
                            left && right);

                        break;
                    }

                case OpCode.Or:
                    {
                        bool right =
                            Convert.ToBoolean(
                                _stack.Pop());

                        bool left =
                            Convert.ToBoolean(
                                _stack.Pop());

                        _stack.Push(
                            left || right);

                        break;
                    }

                case OpCode.Not:
                    {
                        bool value =
                            Convert.ToBoolean(
                                _stack.Pop());

                        _stack.Push(
                            !value);

                        break;
                    }

                case OpCode.Jump:
                    {
                        instructionPointer =
                            ReadInt32(
                                bytecode,
                                ref instructionPointer);

                        continue;
                    }

                case OpCode.JumpIfFalse:
                    {
                        bool condition =
                            Convert.ToBoolean(
                                _stack.Pop());

                        int target =
                            ReadInt32(
                                bytecode,
                                ref instructionPointer);

                        if (!condition)
                        {
                            instructionPointer = target;
                            continue;
                        }

                        break;
                    }

                case OpCode.CallFunction:
                    {
                        ushort index =
                            ReadUInt16(
                                bytecode,
                                ref instructionPointer);

                        byte argumentCount =
                            bytecode[instructionPointer++];

                        var args =
                            new object?[argumentCount];

                        for (int i = argumentCount - 1; i >= 0; i--)
                        {
                            args[i] =
                                _stack.Pop();
                        }

                        object? result =
                            _vmFunctions?.Invoke(
                                index,
                                args);

                        _stack.Push(result);

                        break;
                    }

                case OpCode.Pop:
                    _stack.Pop();
                    break;

                case OpCode.End:
                    return;

                default:
                    throw new Exception(
                        $"Unsupported opcode: {opcode}");
            }
        }
    }

    private int GetInt(
        ushort address)
    {
        EnsureMemory(
            address,
            4);

        return BitConverter.ToInt32(
            _memory,
            address);
    }

    private void SetInt(
        ushort address,
        int value)
    {
        EnsureMemory(
            address,
            4);

        byte[] bytes =
            BitConverter.GetBytes(value);

        Buffer.BlockCopy(
            bytes,
            0,
            _memory,
            address,
            4);
    }

    private float GetFloat(
        ushort address)
    {
        EnsureMemory(
            address,
            4);

        return BitConverter.ToSingle(
            _memory,
            address);
    }

    private void SetFloat(
        ushort address,
        float value)
    {
        EnsureMemory(
            address,
            4);

        byte[] bytes =
            BitConverter.GetBytes(value);

        Buffer.BlockCopy(
            bytes,
            0,
            _memory,
            address,
            4);
    }

    private bool GetBool(
        ushort address)
    {
        EnsureMemory(
            address,
            1);

        return _memory[address] != 0;
    }

    private void SetBool(
        ushort address,
        bool value)
    {
        EnsureMemory(
            address,
            1);

        _memory[address] =
            value ? (byte)1 : (byte)0;
    }

    private void EnsureMemory(
        ushort address,
        int size)
    {
        if (address + size > _memory.Length)
        {
            throw new Exception(
                $"Memory access out of range: {address}");
        }
    }

    private void Compare(
    Func<dynamic, dynamic, bool> operation)
    {
        dynamic right = _stack.Pop()!;
        dynamic left = _stack.Pop()!;

        _stack.Push(
            operation(left, right));
    }

    private void BinaryNumeric(
        Func<dynamic, dynamic, dynamic> operation)
    {
        dynamic right = _stack.Pop()!;
        dynamic left = _stack.Pop()!;

        _stack.Push(
            operation(left, right));
    }

    private static ushort ReadUInt16(
        byte[] bytecode,
        ref int instructionPointer)
    {
        ushort value =
            BitConverter.ToUInt16(
                bytecode,
                instructionPointer);

        instructionPointer += 2;

        return value;
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
}
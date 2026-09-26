using NTOSCompiler.Compiler;
using System.Diagnostics;


namespace NTOSCompiler;

public sealed class VirtualMachine
{
    private readonly Stack<object?> _stack = new();
    private readonly Stack<ushort> _returnStack = new();

    private readonly VMFunctions _vmFunctions;

    private readonly byte[] _memory;


    private Stopwatch _stopwatch = new Stopwatch();
    private CancellationTokenSource? _executionCts;

    public bool Initialized = false;

    public VirtualMachine(
        int memorySize = 4096,
        VMFunctions vmFunctions = null)
    {
        _vmFunctions = vmFunctions;
        _memory = new byte[memorySize];
    }

    public void Stop()
    {
        _executionCts?.Cancel();
    }

    public async Task Execute(byte[] bytecode)
    {
        Stop();

        var cts = new CancellationTokenSource();
        _executionCts = cts;

        try
        {
            await ExecuteAsync(bytecode, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected when execution is stopped.
        }
        finally
        {
            if (ReferenceEquals(_executionCts, cts))
            {
                _executionCts = null;
            }

            cts.Dispose();
        }
    }



    private async Task ExecuteAsync(byte[] bytecode, CancellationToken cancellationToken)
    {
        // _isRunning = true;
        _stack.Clear();
        _returnStack.Clear();

        int instructionPointer = 0;
        _stopwatch.Restart();

        while (instructionPointer < bytecode.Length)
        {
            if (_stopwatch.ElapsedMilliseconds >= 1000)
            {
                throw new Exception(
                    "VM execution limit exceeded. Possible infinite loop.");
            }

            cancellationToken.ThrowIfCancellationRequested();

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
                case OpCode.PushByte:
                    {
                        byte value = bytecode[instructionPointer++];
                        _stack.Push(value);
                        break;
                    }
                case OpCode.PushStr:
                    {
                        int start = instructionPointer;

                        while (instructionPointer < bytecode.Length &&
                               bytecode[instructionPointer] != 0)
                        {
                            instructionPointer++;
                        }

                        if (instructionPointer >= bytecode.Length)
                        {
                            throw new InvalidOperationException(
                                "Unterminated string literal in bytecode.");
                        }

                        string value = System.Text.Encoding.ASCII.GetString(
                            bytecode,
                            start,
                            instructionPointer - start);

                        instructionPointer++; // Skip '\0'

                        _stack.Push(value);
                        break;
                    }

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

                case OpCode.LoadByte:
                    {
                        ushort address =
                            ReadAddress(
                                bytecode,
                                ref instructionPointer);

                        _stack.Push(
                            GetByte(address));

                        break;
                    }

                case OpCode.StoreByte:
                    {
                        ushort address =
                            ReadAddress(
                                bytecode,
                                ref instructionPointer);

                        byte value =
                            Convert.ToByte(_stack.Pop());

                        SetByte(address, value);

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
                case OpCode.JumpIfInitialized:
                    {
                        if (Initialized)
                        {
                            instructionPointer =
                            ReadInt32(
                                bytecode,
                                ref instructionPointer);


                        }
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
                case OpCode.Checkpoint:
                    _stopwatch.Restart();
                    break;
                case OpCode.Return:
                    {
                        instructionPointer = _returnStack.Pop();
                        break;
                    }
                case OpCode.CallSubroutine:
                    {
                        ushort targetAddress =
                            ReadAddress(
                                bytecode,
                                ref instructionPointer);

                        _returnStack.Push(
                            checked((ushort)instructionPointer));

                        instructionPointer = targetAddress;

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
                case OpCode.Delay:
                    {
                        float seconds = ReadFloat(
                            bytecode,
                            ref instructionPointer);

                        await Task.Delay(
                            TimeSpan.FromSeconds(seconds),
                            _executionCts.Token);
                        _stopwatch.Restart();
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

    private byte GetByte(ushort address)
    {
        EnsureMemory(address, 1);

        return _memory[address];
    }

    private void SetByte(ushort address, byte value)
    {
        EnsureMemory(address, 1);

        _memory[address] = value;
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

    public void WriteByteToMemory(int address, byte value)
    {
        _memory[address] = value;
    }
}
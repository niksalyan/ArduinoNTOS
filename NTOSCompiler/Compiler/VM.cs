namespace NTOSCompiler;

public sealed class VirtualMachine
{
    private readonly Dictionary<string, object?> _variables = new();
    private readonly Stack<object?> _stack = new();

    private readonly FunctionRegistry _functions;

    public VirtualMachine(FunctionRegistry functions)
    {
        _functions = functions;
    }

    public object? Execute(BytecodeProgram program)
    {
        _stack.Clear();

        int instructionPointer = 0;

        while (instructionPointer < program.Instructions.Count)
        {
            var instruction = program.Instructions[instructionPointer];

            switch (instruction.OpCode)
            {
                case OpCode.PushInt:
                case OpCode.PushFloat:
                case OpCode.PushString:
                    _stack.Push(instruction.Operand);
                    break;

                case OpCode.PushBool:
                    _stack.Push(instruction.Operand);
                    break;

                case OpCode.LoadVariable:
                    {
                        string name = (string)instruction.Operand!;

                        if (!_variables.TryGetValue(name, out var value))
                            throw new Exception(
                                $"Variable '{name}' is not defined.");

                        _stack.Push(value);
                        break;
                    }

                case OpCode.StoreVariable:
                    {
                        string name = (string)instruction.Operand!;

                        _variables[name] = _stack.Pop();
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
                            Convert.ToInt32(instruction.Operand);

                        continue;
                    }

                case OpCode.JumpIfFalse:
                    {
                        bool condition =
                            Convert.ToBoolean(_stack.Pop());

                        if (!condition)
                        {
                            instructionPointer =
                                Convert.ToInt32(instruction.Operand);

                            continue;
                        }

                        break;
                    }

                case OpCode.CallFunction:
                    {
                        var call = (FunctionCall)instruction.Operand!;

                        var args =
                            new object?[call.ArgumentCount];

                        for (int i = call.ArgumentCount - 1; i >= 0; i--)
                            args[i] = _stack.Pop();

                        var function =
                            _functions.GetFunction(call.Name);

                        object? result = function(args);

                        _stack.Push(result);
                        break;
                    }

                case OpCode.Pop:
                    _stack.Pop();
                    break;

                default:
                    throw new Exception(
                        $"Unsupported opcode: {instruction.OpCode}");
            }

            instructionPointer++;
        }

        return _stack.Count > 0
            ? _stack.Peek()
            : null;
    }

    private void Compare(
        Func<double, double, bool> operation)
    {
        double right =
            Convert.ToDouble(_stack.Pop());

        double left =
            Convert.ToDouble(_stack.Pop());

        _stack.Push(
            operation(left, right));
    }

    public object? GetVariable(string name)
        => _variables.TryGetValue(
            name,
            out var value)
                ? value
                : null;

    private void BinaryNumeric(
        Func<double, double, double> operation)
    {
        double right =
            Convert.ToDouble(_stack.Pop());

        double left =
            Convert.ToDouble(_stack.Pop());

        _stack.Push(
            operation(left, right));
    }
}

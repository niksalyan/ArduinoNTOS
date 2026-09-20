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

        foreach (var instruction in program.Instructions)
        {
            switch (instruction.OpCode)
            {
                case OpCode.PushInt:
                case OpCode.PushFloat:
                case OpCode.PushString:
                    _stack.Push(instruction.Operand);
                    break;

                case OpCode.LoadVariable:
                {
                    string name = (string)instruction.Operand!;

                    if (!_variables.TryGetValue(name, out var value))
                        throw new Exception($"Variable '{name}' is not defined.");

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

                case OpCode.Pop:
                    _stack.Pop();
                    break;
                case OpCode.CallFunction:
                    var call = (FunctionCall)instruction.Operand!;

                    var args = new object?[call.ArgumentCount];

                    for (int i = call.ArgumentCount - 1; i >= 0; i--)
                        args[i] = _stack.Pop();

                    var function = _functions.GetFunction(call.Name);

                    object? result = function(args);

                    _stack.Push(result);
                    break;
                default:
                    throw new Exception($"Unsupported opcode: {instruction.OpCode}");
            }
        }

        return _stack.Count > 0 ? _stack.Peek() : null;
    }

    public object? GetVariable(string name)
        => _variables.TryGetValue(name, out var value)
            ? value
            : null;

    private void BinaryNumeric(Func<double, double, double> operation)
    {
        double right = Convert.ToDouble(_stack.Pop());
        double left = Convert.ToDouble(_stack.Pop());

        _stack.Push(operation(left, right));
    }
}

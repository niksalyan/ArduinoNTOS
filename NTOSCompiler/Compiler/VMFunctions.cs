
using System.Diagnostics;

namespace NTOSCompiler.Compiler
{
    public class VMFunctions
    {
        private Dictionary<string, int> _indexes = new Dictionary<string, int>();

        private Dictionary<int, int> _argsCount = new Dictionary<int, int>();
        private Dictionary<string, VariableType> _returnTypes = new Dictionary<string, VariableType>();
        private readonly Dictionary<int, Func<object[], Task<object>>> _functions = new();

        public void AddFunction(int index, string name, int argsCount, VariableType returnType, Func<object[], Task<object>> func)
        {
            if (_functions.ContainsKey(index))
            {
                Debug.WriteLine("Duplicate function definintion");
                return;
            }
            _indexes[name] = index;
            _argsCount[index] = argsCount;
            _returnTypes[name] = returnType;
            _functions[index] = func;
        }

        public async Task<object> InvokeAsync(int index, object[] args)
        {
            try
            {
                if (_argsCount[index] != args.Length)
                {
                    throw new Exception("Invalid arguments count");
                }
                return await _functions[index](args);
            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int GetIndex(string name)
        {
            return _indexes[name];
        }

        public VariableType GetReturnType(string name)
        {
            return _returnTypes[name];
        }

    }

    public readonly record struct FunctionCall(
    ushort Index,
    byte ArgumentCount);
}
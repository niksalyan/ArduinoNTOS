using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NTOSCompiler.Compiler
{
    public class VMFunctions
    {
        private Dictionary<string, int> _indexes = new Dictionary<string, int>();
        private Dictionary<string, VariableType> _returnTypes = new Dictionary<string, VariableType>();
        private readonly Dictionary<int, Func<object[], Task<object>>> _functions = new();

        public void AddFunction(int index, string name, VariableType returnType, Func<object[], Task<object>> func)
        {
            _indexes[name] = index;
            _returnTypes[name] = returnType;
            _functions[index] = func;
        }

        public async Task<object> InvokeAsync(int index, object[] args)
        {
            return await _functions[index](args);
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
}
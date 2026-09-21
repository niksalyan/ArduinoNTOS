using System;
using System.Collections.Generic;
using System.Text;

namespace NTOSCompiler.Compiler
{
    public class VMFunctions
    {
        private Dictionary<string, int> _indexes = new Dictionary<string, int>();
        private Dictionary<int, VariableType> _returnTypes = new Dictionary<int, VariableType>();
        private Dictionary<int, Func<object[], object>> _functions = new Dictionary<int, Func<object[], object>>();


        public void AddFunction(int index, string name, VariableType returnType, Func<object[], object> func)
        {
            _indexes[name] = index;
            _returnTypes[index] = returnType;
            _functions[index] = func;
        }

        public object Invoke(int index, object[] args)
        {
            return _functions[index](args);
        }

        public int GetIndex(string name)
        {
            return _indexes[name];
        }

        public VariableType GetReturnType(int index)
        {
            return _returnTypes[index];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NTOSCompiler.Compiler
{
    public class VMFunctions
    {
        private Dictionary<string, int> _indexes = new Dictionary<string, int>();
        private Dictionary<int, Func<object[], object>> _functions = new Dictionary<int, Func<object[], object>>();


        public void AddFunction(int index, string name, Func<object[], object> func)
        {
            _indexes[name] = index;
            _functions[index] = func;
        }

        public object Invoke(int index, object[] args)
        {
            Debug.WriteLine("Invoke: " + index);
            return _functions[index](args);
        }

        public int GetIndex(string name)
        {
            return _indexes[name];
        }

    }
}
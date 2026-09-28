using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NTOSCompiler.Compiler
{
    public class VMFunctions
    {
        private Dictionary<string, int> _indexes = new Dictionary<string, int>();
        private readonly Dictionary<int, Func<object[], Task<object>>> _functions = new();

        public void AddFunction(int index, string name, Func<object[], Task<object>> func)
        {
            _indexes[name] = index;
            _functions[index] = func;
        }

        public async Task<object> InvokeAsync(int index, object[] args)
        {
            try
            {
                return await _functions[index](args);
            }
            catch
            {
                return null;
            }
        }

        public int GetIndex(string name)
        {
            return _indexes[name];
        }

    }
}
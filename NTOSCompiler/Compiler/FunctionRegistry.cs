using System;
using System.Collections.Generic;
using System.Text;

namespace NTOSCompiler
{

    public delegate object? NativeFunction(object?[] args);
    public sealed class FunctionRegistry
    {
        private readonly Dictionary<string, NativeFunction> _functions = new();

        public void RegisterFunction(string name, NativeFunction function)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Function name cannot be empty.", nameof(name));

            if (function == null)
                throw new ArgumentNullException(nameof(function));

            _functions[name] = function;
        }

        public NativeFunction GetFunction(string name)
        {
            if (!_functions.TryGetValue(name, out var function))
                throw new Exception($"Function '{name}' is not registered.");

            return function;
        }

        public bool Contains(string name)
            => _functions.ContainsKey(name);
    }
}

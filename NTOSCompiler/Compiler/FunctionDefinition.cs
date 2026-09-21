using System;
using System.Collections.Generic;
using System.Text;

namespace NTOSCompiler.Compiler
{
    public sealed class FunctionDefinition
    {
        public string Name { get; }

        public VariableType ReturnType { get; }
        public ParameterDefinition[] Parameters { get; }
        public int EntryPoint { get; set; }

        public FunctionDefinition(
            string name,
            VariableType returnType,
            ParameterDefinition[] parameters,
            int entryPoint)
        {
            Name = name;
            ReturnType = returnType;
            Parameters = parameters;
            EntryPoint = entryPoint;
        }
    }

    public sealed class ParameterDefinition
    {
        public string Name { get; }
        public VariableType Type { get; }

        public ParameterDefinition(
            string name,
            VariableType type)
        {
            Name = name;
            Type = type;
        }
    }
}

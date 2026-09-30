using Acornima;
using Acornima.Ast;


namespace NTOSCompiler.Compiler
{
    

    public class Compiler : AstVisitor
    {
        private static readonly Acornima.Parser _parser = new Acornima.Parser();
        private string _src;

        private readonly VMFunctions _vmFunctions;
        private Dictionary<string, byte> _constants;

        private List<Variable> _variables = new List<Variable>();
        private List<Instruction> _instructions = new List<Instruction>();


        public Compiler(VMFunctions vmFunctions, Dictionary<string, byte>? constants)
        {
            _vmFunctions = vmFunctions ?? new VMFunctions();
            _constants = constants ?? new();
        }

        public byte[] Compile(string src)
        {
            _instructions.Clear();
            _src = src;
            Node ast = _parser.ParseScript(_src);
            Visit(ast);

            Add(OpCode.End);

            return new BytecodeProgram(_instructions).PrepareBytecode();        }

        private void Add(OpCode opCode, object? operand = null)
        {
            _instructions.Add(new Instruction(opCode, operand));
        }


        public Variable GetVariable(string name)
        {
            var variable = _variables.FirstOrDefault(x => x.Name == name);

            if (variable == null)
                throw new InvalidOperationException(
                    $"Variable '{name}' is not declared.");

            return variable;
        }

        public Variable DeclareVariable(
        string name,
        VariableType type,
        bool isArray = false,
        int length = 1,
        int maxStringLength = 0)
        {
            var variable = _variables.FirstOrDefault(x => x.Name == name);
            if (variable != null)
            {
                return variable;
            }

            variable = new Variable(
                name,
                type,
                isArray,
                length,
                maxStringLength);

            _variables.Add(variable);
            UpdateAddresses();

            return variable;
        }


        private void UpdateAddresses()
        {
            int address = 0;
            foreach (var variable in _variables)
            {
                variable.Address = address;
                address += variable.GetSize();
            }
        }

        public string GetText(Acornima.Range range)
        {
            return _src.Substring(range.Start, range.Length);
        }
    }
}

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


        public List<Variable> Variables => _variables;
        public List<Instruction> Instructions => _instructions;


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

            return new BytecodeProgram(_instructions).PrepareBytecode();        
        }

        // ------------------------------------------------------------
        // Statements
        // ------------------------------------------------------------

        protected override object? VisitExpressionStatement(
            ExpressionStatement expressionStatement)
        {
            Visit(expressionStatement.Expression);
            return null;
        }

        protected override object? VisitVariableDeclaration(
            VariableDeclaration variableDeclaration)
        {
            foreach (var declaration in variableDeclaration.Declarations)
                Visit(declaration);

            return null;
        }

        protected override object? VisitVariableDeclarator(
            VariableDeclarator variableDeclarator)
        {
            if (variableDeclarator.Id is not Identifier identifier)
                throw new InvalidOperationException(
                    "Only simple variable declarations are supported.");

            string name = identifier.Name;

            // No initializer:
            // let a;
            // var a;
            if (variableDeclarator.Init == null)
                return null;

            VariableType type = GetExpressionType(variableDeclarator.Init);

            Variable variable = DeclareVariable(name, type);

            // let: existing variable is left untouched.
            // var: existing variable gets overwritten.
            //
            // Since we don't know whether DeclareVariable created it,
            // check the variable list directly.

            bool alreadyExists = _variables.Any(x =>
                x.Name == name &&
                ReferenceEquals(x, variable));

            // We need to distinguish creation from an existing variable.
            // Replace this logic with Contains-before-Declare if preferred.
            CompileExpression(variableDeclarator.Init);

            Store(variable);

            return null;
        }

        private VariableType GetExpressionType(Expression expression)
        {
            if (expression is Literal literal)
                return GetLiteralType(literal);

            if (expression is Identifier identifier)
                return GetVariable(identifier.Name).Type;

            throw new InvalidOperationException(
                $"Cannot determine type of expression: {expression.GetType().Name}");
        }

        private VariableType GetLiteralType(Literal literal)
        {
            if (literal.Value is bool)
                return VariableType.Bool;

            if (literal.Value is string)
                return VariableType.Str;

            if (literal.Value is int ||
                literal.Value is long)
                return VariableType.Int;

            if (literal.Value is double)
            {
                string text = GetText(literal.Range);

                return text.Contains('.') ||
                       text.Contains('e') ||
                       text.Contains('E')
                    ? VariableType.Float
                    : VariableType.Int;
            }

            throw new InvalidOperationException(
                $"Unsupported literal: {literal.Value}");
        }

        private void CompileExpression(Expression expression)
        {
            Visit(expression);
        }

        private void Store(Variable variable)
        {
            switch (variable.Type)
            {
                case VariableType.Int:
                    Add(OpCode.StoreInt, variable.Address);
                    break;

                case VariableType.Float:
                    Add(OpCode.StoreFloat, variable.Address);
                    break;

                case VariableType.Byte:
                case VariableType.Bool:
                    Add(OpCode.StoreByte, variable.Address);
                    break;

                case VariableType.Str:
                    Add(OpCode.StoreStr, variable.Address);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Cannot store variable type {variable.Type}.");
            }
        }

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

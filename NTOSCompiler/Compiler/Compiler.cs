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

        public void ClearVariables()
        {
            _variables.Clear();
        }

        public byte[] Compile(string src)
        {
            _instructions.Clear();
            _src = src;
            Node ast = _parser.ParseScript(_src);
            Visit(ast);

            Add(OpCode.End);

            var program = new BytecodeProgram(_variables, _instructions);
            program.PrepareBytecode(true); //  We do 2 pass bytecode generation first pass assigns addresses to the instructions
            return program.PrepareBytecode();
        }

        // ------------------------------------------------------------
        // Statements
        // ------------------------------------------------------------
        protected override object? VisitIfStatement(
    IfStatement ifStatement)
        {
            // Compile condition.
            Visit(ifStatement.Test);

            // Reserve JumpIfFalse.
            int jumpIfFalseIndex = _instructions.Count;
            Add(OpCode.JumpIfFalse, 0);

            // Compile "then" branch.
            Visit(ifStatement.Consequent);

            if (ifStatement.Alternate != null)
            {
                // Reserve Jump over the else branch.
                int jumpEndIndex = _instructions.Count;
                Add(OpCode.Jump, 0);

                // The next instruction is the start of else.
                int elseIndex = _instructions.Count;

                _instructions[jumpIfFalseIndex] =
                    new Instruction(OpCode.JumpIfFalse, elseIndex);

                // Compile else / else-if.
                Visit(ifStatement.Alternate);

                // The instruction after the else branch.
                int endIndex = _instructions.Count;

                _instructions[jumpEndIndex] =
                    new Instruction(OpCode.Jump, endIndex);
            }
            else
            {
                // No else: false goes directly after the then branch.
                int endIndex = _instructions.Count;

                _instructions[jumpIfFalseIndex] =
                    new Instruction(OpCode.JumpIfFalse, endIndex);
            }

            return null;
        }

        protected override object? VisitWhileStatement(
    WhileStatement whileStatement)
        {
            // The condition must be the first instruction of the loop.
            int loopStartIndex = _instructions.Count;

            // Compile condition.
            Visit(whileStatement.Test);

            // Reserve the exit jump.
            int jumpIfFalseIndex = _instructions.Count;
            Add(OpCode.JumpIfFalse, 0);

            // Compile loop body.
            Visit(whileStatement.Body);

            // Jump back to the condition.
            Add(OpCode.Jump, loopStartIndex);

            // False condition exits after the loop body.
            int endIndex = _instructions.Count;

            _instructions[jumpIfFalseIndex] =
                new Instruction(OpCode.JumpIfFalse, endIndex);

            return null;
        }

        protected override object? VisitCallExpression(
    CallExpression callExpression)
        {
            if (callExpression.Callee is not Identifier identifier)
                throw new InvalidOperationException(
                    "Only named function calls are supported.");

            string functionName = identifier.Name;

            if (_vmFunctions == null)
            {
                throw new InvalidOperationException(
                    "VMFunctions is required to compile function calls.");
            }

            int argumentCount = 0;

            foreach (var argument in callExpression.Arguments)
            {
                Visit(argument);
                argumentCount++;
            }

            int functionIndex =
                _vmFunctions.GetIndex(functionName);

            Add(
                OpCode.CallFunction,
                new FunctionCall(
                    (ushort)functionIndex,
                    (byte)argumentCount));

            return _vmFunctions.GetReturnType(functionName);
        }

        protected override object? VisitDoWhileStatement(
    DoWhileStatement doWhileStatement)
        {
            int loopStartIndex = _instructions.Count;

            Visit(doWhileStatement.Body);
            Visit(doWhileStatement.Test);

            int jumpIfFalseIndex = _instructions.Count;
            Add(OpCode.JumpIfFalse, 0);

            Add(OpCode.Jump, loopStartIndex);

            int endIndex = _instructions.Count;

            _instructions[jumpIfFalseIndex] =
                new Instruction(OpCode.JumpIfFalse, endIndex);

            return null;
        }

        protected override object? VisitExpressionStatement(
            ExpressionStatement expressionStatement)
        {
            Visit(expressionStatement.Expression);
            return null;
        }

        protected override object? VisitVariableDeclaration(
    VariableDeclaration variableDeclaration)
        {
            bool isLet = variableDeclaration.Kind == VariableDeclarationKind.Let;
            bool isVar = variableDeclaration.Kind == VariableDeclarationKind.Var;

            foreach (var declaration in variableDeclaration.Declarations)
            {
                CompileVariableDeclarator(declaration, isLet, isVar);
            }

            return null;
        }

        private void CompileVariableDeclarator(
            VariableDeclarator variableDeclarator,
            bool isLet,
            bool isVar)
        {
            if (variableDeclarator.Id is not Identifier identifier)
                throw new InvalidOperationException(
                    "Only simple variable declarations are supported.");

            string name = identifier.Name;

            Variable? existing = _variables
                .FirstOrDefault(x => x.Name == name);

            // let + existing variable:
            // completely ignore this declaration.
            if (existing != null && isLet)
                return;

            // No initializer.
            if (variableDeclarator.Init == null)
                return;

            VariableType type =
                GetExpressionType(variableDeclarator.Init);

            // var + existing variable:
            // overwrite the existing value.
            if (existing != null)
            {
                if (existing.Type != type)
                    throw new InvalidOperationException(
                        $"Variable '{name}' is {existing.Type}, " +
                        $"but initializer is {type}.");

                CompileExpression(variableDeclarator.Init);
                Store(existing);

                return;
            }

            // New variable.
            Variable variable =
                DeclareVariable(name, type);

            CompileExpression(variableDeclarator.Init);
            Store(variable);
        }

        private VariableType GetExpressionType(Expression expression)
        {
            if (expression is Literal literal)
                return GetLiteralType(literal);

            if (expression is Identifier identifier)
                return GetVariable(identifier.Name).Type;

            if (expression is BinaryExpression binary)
                return GetBinaryExpressionType(binary);

            throw new InvalidOperationException(
                $"Cannot determine type of expression: {expression.GetType().Name}");
        }

        private VariableType GetBinaryExpressionType(
                BinaryExpression expression)
        {
            switch (expression.Operator)
            {
                case Operator.Addition:
                case Operator.Subtraction:
                case Operator.Multiplication:
                case Operator.Division:
                case Operator.Remainder:
                    {
                        VariableType left =
                            GetExpressionType(expression.Left);

                        VariableType right =
                            GetExpressionType(expression.Right);

                        if (left == VariableType.Float ||
                            right == VariableType.Float)
                        {
                            return VariableType.Float;
                        }

                        if (left == VariableType.Int &&
                            right == VariableType.Int)
                        {
                            return VariableType.Int;
                        }

                        throw new InvalidOperationException(
                            $"Invalid arithmetic types: {left} and {right}.");
                    }

                case Operator.Equality:
                case Operator.Inequality:
                case Operator.LessThan:
                case Operator.GreaterThan:
                case Operator.LessThanOrEqual:
                case Operator.GreaterThanOrEqual:
                case Operator.LogicalAnd:
                case Operator.LogicalOr:
                    return VariableType.Bool;

                default:
                    throw new InvalidOperationException(
                        $"Cannot determine type of operator: {expression.Operator}");
            }
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

        protected override object? VisitBinaryExpression(
    BinaryExpression expression)
        {
            Visit(expression.Left);
            Visit(expression.Right);

            Add(expression.Operator switch
            {
                Operator.Addition => OpCode.Add,
                Operator.Subtraction => OpCode.Subtract,
                Operator.Multiplication => OpCode.Multiply,
                Operator.Division => OpCode.Divide,
                Operator.Remainder => OpCode.Modulo,

                Operator.Equality => OpCode.Equal,
                Operator.Inequality => OpCode.NotEqual,
                Operator.LessThan => OpCode.Less,
                Operator.GreaterThan => OpCode.Greater,
                Operator.LessThanOrEqual => OpCode.LessEqual,
                Operator.GreaterThanOrEqual => OpCode.GreaterEqual,

                Operator.LogicalAnd => OpCode.And,
                Operator.LogicalOr => OpCode.Or,

                _ => throw new InvalidOperationException(
                    $"Unsupported binary operator: {expression.Operator}")
            });

            return null;
        }


        protected override object? VisitLiteral(Literal literal)
        {
            switch (GetLiteralType(literal))
            {
                case VariableType.Int:
                    Add(OpCode.PushInt, Convert.ToInt32(literal.Value));
                    break;

                case VariableType.Float:
                    Add(OpCode.PushFloat, Convert.ToSingle(literal.Value));
                    break;

                case VariableType.Bool:
                    Add(
                        OpCode.PushByte,
                        (bool)literal.Value ? 1 : 0);
                    break;

                case VariableType.Str:
                    Add(
                        OpCode.PushStr,
                        Convert.ToString(literal.Value) ?? string.Empty);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported literal: {literal.Value}");
            }

            return null;
        }

        protected override object? VisitIdentifier(Identifier identifier)
        {
            string name = identifier.Name;

            if (_constants.ContainsKey(name))
            {
                Add(OpCode.PushByte, _constants[name]);
                return null;
            }
            else if (name.Length > 1 &&
                name[0] == 'b' &&
                byte.TryParse(name.AsSpan(1), out byte byteValue))
            {
                Add(OpCode.PushByte, byteValue);
                return null;
            }

            Variable variable = GetVariable(name);

            Load(variable);

            return null;
        }

        protected override object? VisitAssignmentExpression(
            AssignmentExpression expression)
        {
            if (expression.Left is not Identifier identifier)
                throw new InvalidOperationException(
                    "Only simple variable assignment is supported.");

            Variable variable = GetVariable(identifier.Name);

            if (expression.Operator != Operator.Assignment)
                throw new InvalidOperationException(
                    $"Unsupported assignment operator: {expression.Operator}");

            VariableType type = GetExpressionType(expression.Right);

            if (type != variable.Type)
                throw new InvalidOperationException(
                    $"Cannot assign {type} to variable '{variable.Name}' of type {variable.Type}.");

            CompileExpression(expression.Right);
            Store(variable);

            return null;
        }

        private void Load(Variable variable)
        {
            switch (variable.Type)
            {
                case VariableType.Int:
                    Add(OpCode.LoadInt, variable.Address);
                    break;

                case VariableType.Float:
                    Add(OpCode.LoadFloat, variable.Address);
                    break;

                case VariableType.Byte:
                case VariableType.Bool:
                    Add(OpCode.LoadByte, variable.Address);
                    break;

                case VariableType.Str:
                    Add(OpCode.LoadStr, variable.Address);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Cannot load variable type {variable.Type}.");
            }
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

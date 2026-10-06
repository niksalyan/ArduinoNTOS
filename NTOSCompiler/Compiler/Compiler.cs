using Acornima;
using Acornima.Ast;
using System.Diagnostics;


namespace NTOSCompiler.Compiler
{


    public class Compiler : AstVisitor
    {
        private static readonly Parser _parser = new Parser(new ParserOptions()
        {
            AllowTopLevelUsing = true
        });
        private string _src;

        private readonly VMFunctions _vmFunctions;
        private Dictionary<string, object> _constantsDefault;
        private Dictionary<string, object> _constants;

        private List<Variable> _variables = new List<Variable>();
        private List<Instruction> _instructions = new List<Instruction>();

        private readonly Dictionary<string, int> _functions = new();
        private readonly Stack<List<int>> _breakJumps = new();
        public List<Variable> Variables => _variables;
        public List<Instruction> Instructions => _instructions;


        public Compiler(VMFunctions vmFunctions, Dictionary<string, object>? constantsDefault)
        {
            _vmFunctions = vmFunctions ?? new VMFunctions();
            _constantsDefault = constantsDefault ?? new();
        }

        public void ClearVariables()
        {
            _variables.Clear();
            _constants = new Dictionary<string, object>(_constantsDefault);
        }

        private void ClearTrailingVarVariables()
        {
            while (_variables.Count > 0)
            {
                Variable variable = _variables[^1];

                if (variable.Kind != VariableDeclarationKind.Var)
                    break;

                _variables.RemoveAt(_variables.Count - 1);
            }
        }

        private void MoveVarVariablesToEnd()
        {
            int insertIndex = 0;

            for (int i = 0; i < _variables.Count; i++)
            {
                if (_variables[i].Kind != VariableDeclarationKind.Var)
                {
                    if (i != insertIndex)
                    {
                        Variable variable = _variables[i];

                        for (int j = i; j > insertIndex; j--)
                            _variables[j] = _variables[j - 1];

                        _variables[insertIndex] = variable;
                    }

                    insertIndex++;
                }
            }
        }

        public byte[] CompileSingle(string src)
        {
            var input = new Dictionary<string, string>() { { "", src } };
            var output = CompileMultiple(input);
            return output[0].PrepareBytecode();
        }

        public List<BytecodeProgram> CompileMultiple(Dictionary<string, string> sourcesDict) 
        {
            var sources = sourcesDict
                    .OrderBy(x => !string.Equals(x.Key, "main", StringComparison.OrdinalIgnoreCase));
            var programs = new List<BytecodeProgram>();
            // pass 1
            foreach (var src in sources)
            {
                CompileInstructions(src.Value);
            }

            MoveVarVariablesToEnd();

            // pass 2
            foreach (var src in sources)
            {
                CompileInstructions(src.Value);
                var program = new BytecodeProgram(src.Key, _variables, _instructions);
                program.PrepareBytecode(true); //  We do 2 pass bytecode generation first pass assigns addresses to the instructions
                programs.Add(program);
            }

            return programs;
        }

        private void CompileInstructions(string src)
        {
            _instructions = new List<Instruction>();
            _functions.Clear();
            _breakJumps.Clear();
            ClearTrailingVarVariables();
            _src = src;
            _constants = _constants ?? new Dictionary<string, object>(_constantsDefault);
            Node ast = _parser.ParseScript(_src);
            Visit(ast);

            Add(OpCode.End);            
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

        protected override object? VisitConditionalExpression(ConditionalExpression conditional)
        {
            // Compile test
            Visit(conditional.Test);

            // Reserve JumpIfFalse to else branch
            int jumpIfFalseIndex = _instructions.Count;
            Add(OpCode.JumpIfFalse, 0);

            // Compile consequent (true branch)
            Visit(conditional.Consequent);

            // Jump over alternate after consequent
            int jumpIndex = _instructions.Count;
            Add(OpCode.Jump, 0);

            // Else branch start
            int elseIndex = _instructions.Count;
            _instructions[jumpIfFalseIndex] =
                new Instruction(OpCode.JumpIfFalse, elseIndex);

            // Compile alternate (false branch)
            Visit(conditional.Alternate);

            // End
            int endIndex = _instructions.Count;
            _instructions[jumpIndex] =
                new Instruction(OpCode.Jump, endIndex);

            return null;
        }

        protected override object? VisitSwitchStatement(
    SwitchStatement switchStatement)
        {
            // ------------------------------------------------------------
            // NTOS switch implementation
            //
            // The switch expression is currently required to be an
            // identifier. This lets us load it again for each case
            // without requiring a DUP opcode or a temporary variable.
            // ------------------------------------------------------------

            if (switchStatement.Discriminant is not Identifier)
            {
                throw new InvalidOperationException(
                    "Switch expression must be a variable.");
            }

            // Breaks belonging to this switch.
            var breakJumps = new List<int>();
            _breakJumps.Push(breakJumps);

            // Information about each case.
            var cases = new List<(SwitchCase Case, int TrueJumpIndex)>();

            SwitchCase? defaultCase = null;

            // ------------------------------------------------------------
            // Phase 1:
            // Generate the case-dispatch code.
            //
            // Example:
            //
            // Load key
            // Push 'A'
            // Equal
            // JumpIfFalse nextTest
            // Jump caseA
            //
            // Load key
            // Push 'B'
            // Equal
            // JumpIfFalse default
            // Jump caseB
            // ------------------------------------------------------------

            var falseJumps = new List<int>();

            foreach (SwitchCase switchCase in switchStatement.Cases)
            {
                // default has no test.
                if (switchCase.Test == null)
                {
                    defaultCase = switchCase;
                    continue;
                }

                // The previous case failed.
                // Its JumpIfFalse must continue here.
                int currentTestIndex = _instructions.Count;

                foreach (int jumpIndex in falseJumps)
                {
                    _instructions[jumpIndex] =
                        new Instruction(
                            OpCode.JumpIfFalse,
                            currentTestIndex);
                }

                falseJumps.Clear();

                // Load switch value.
                Visit(switchStatement.Discriminant);

                // Load case value.
                Visit(switchCase.Test);

                // Compare.
                Add(OpCode.Equal);

                // If false, test the next case.
                int jumpIfFalseIndex = _instructions.Count;

                Add(OpCode.JumpIfFalse, 0);

                falseJumps.Add(jumpIfFalseIndex);

                // If true, jump to this case's body.
                int jumpToBodyIndex = _instructions.Count;

                Add(OpCode.Jump, 0);

                cases.Add(
                    (switchCase, jumpToBodyIndex));
            }

            // ------------------------------------------------------------
            // Phase 2:
            // The final failed comparison goes to default,
            // or to the end if there is no default.
            // ------------------------------------------------------------

            int bodyStartIndex = _instructions.Count;

            foreach (int jumpIndex in falseJumps)
            {
                _instructions[jumpIndex] =
                    new Instruction(
                        OpCode.JumpIfFalse,
                        bodyStartIndex);
            }

            // ------------------------------------------------------------
            // Phase 3:
            // Emit case bodies in their original order.
            //
            // This is what gives us fall-through behavior.
            //
            // case 'A':
            //     foo();
            //
            // case 'B':
            //     bar();
            //
            // If A matches and doesn't break, execution naturally
            // continues into B.
            // ------------------------------------------------------------

            var caseBodyIndexes =
                new Dictionary<SwitchCase, int>();

            foreach (SwitchCase switchCase in switchStatement.Cases)
            {
                int caseBodyIndex = _instructions.Count;

                caseBodyIndexes[switchCase] = caseBodyIndex;

                foreach (Statement statement in switchCase.Consequent)
                {
                    Visit(statement);
                }
            }

            // ------------------------------------------------------------
            // Switch end.
            // ------------------------------------------------------------

            int endIndex = _instructions.Count;

            // ------------------------------------------------------------
            // Patch case dispatch jumps.
            // ------------------------------------------------------------

            foreach (var entry in cases)
            {
                SwitchCase switchCase = entry.Case;
                int jumpIndex = entry.TrueJumpIndex;

                _instructions[jumpIndex] =
                    new Instruction(
                        OpCode.Jump,
                        caseBodyIndexes[switchCase]);
            }

            // ------------------------------------------------------------
            // Patch failed case comparison.
            //
            // The final JumpIfFalse currently points to the beginning
            // of the body area. It needs to point specifically to
            // default, or the end of the switch.
            // ------------------------------------------------------------

            if (falseJumps.Count > 0)
            {
                int target;

                if (defaultCase != null)
                {
                    target = caseBodyIndexes[defaultCase];
                }
                else
                {
                    target = endIndex;
                }

                foreach (int jumpIndex in falseJumps)
                {
                    _instructions[jumpIndex] =
                        new Instruction(
                            OpCode.JumpIfFalse,
                            target);
                }
            }

            // ------------------------------------------------------------
            // Patch all break statements belonging to this switch.
            // ------------------------------------------------------------

            foreach (int breakJumpIndex in breakJumps)
            {
                _instructions[breakJumpIndex] =
                    new Instruction(
                        OpCode.Jump,
                        endIndex);
            }

            _breakJumps.Pop();

            return null;
        }

        protected override object? VisitBreakStatement(
    BreakStatement breakStatement)
        {
            if (breakStatement.Label != null)
            {
                throw new InvalidOperationException(
                    "Labeled break is not supported.");
            }

            if (_breakJumps.Count == 0)
            {
                throw new InvalidOperationException(
                    "'break' is only valid inside a switch.");
            }

            int jumpIndex = _instructions.Count;

            // Target is patched when the switch ends.
            Add(OpCode.Jump, 0);

            _breakJumps.Peek().Add(jumpIndex);

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

        protected override object? VisitForStatement(
    ForStatement forStatement)
        {
            // for (INIT; TEST; UPDATE)

            // 1. Initialization
            if (forStatement.Init != null)
                Visit(forStatement.Init);

            // 2. Loop condition
            int loopStartIndex = _instructions.Count;

            int jumpIfFalseIndex = -1;

            if (forStatement.Test != null)
            {
                Visit(forStatement.Test);

                jumpIfFalseIndex = _instructions.Count;
                Add(OpCode.JumpIfFalse, 0);
            }

            // 3. Loop body
            Visit(forStatement.Body);

            // 4. Update
            if (forStatement.Update != null)
                Visit(forStatement.Update);

            // 5. Go back to the condition
            Add(OpCode.Jump, loopStartIndex);

            // 6. Exit target
            int endIndex = _instructions.Count;

            if (jumpIfFalseIndex >= 0)
            {
                _instructions[jumpIfFalseIndex] =
                    new Instruction(
                        OpCode.JumpIfFalse,
                        endIndex);
            }

            return null;
        }

        protected override object? VisitFunctionDeclaration(
    FunctionDeclaration functionDeclaration)
        {
            string name = functionDeclaration.Id!.Name;

            if (_functions.ContainsKey(name))
                throw new InvalidOperationException(
                    $"Function '{name}' is already declared.");

            // ------------------------------------------------------------
            // init()
            // ------------------------------------------------------------

            if (name == "init")
            {
                int jumpIfInitializedIndex =
                    _instructions.Count;

                Add(OpCode.JumpIfInitialized, 0);

                _functions[name] =
                    _instructions.Count;

                Visit(functionDeclaration.Body);

                int endIndex2 =
                    _instructions.Count;

                _instructions[jumpIfInitializedIndex] =
                    new Instruction(
                        OpCode.JumpIfInitialized,
                        endIndex2);

                return null;
            }

            // ------------------------------------------------------------
            // loop()
            // ------------------------------------------------------------

            if (name == "loop")
            {
                int loopStartIndex =
                    _instructions.Count;

                _functions[name] =
                    loopStartIndex;

                Visit(functionDeclaration.Body);

                Add(OpCode.Jump, loopStartIndex);

                return null;
            }

            // ------------------------------------------------------------
            // Normal subroutine
            // ------------------------------------------------------------

            // Skip the function body during normal execution.
            int jumpIndex =
                _instructions.Count;

            Add(OpCode.Jump, 0);

            // First instruction of the actual function.
            _functions[name] =
                _instructions.Count;

            Visit(functionDeclaration.Body);

            // User functions have no return value.
            Add(OpCode.Return);

            // Execution continues after the function.
            int endIndex =
                _instructions.Count;

            _instructions[jumpIndex] =
                new Instruction(
                    OpCode.Jump,
                    endIndex);

            return null;
        }


        protected override object? VisitCallExpression(
    CallExpression callExpression)
        {
            if (callExpression.Callee is not Identifier identifier)
                throw new InvalidOperationException(
                    "Only named function calls are supported.");

            string functionName = identifier.Name;

            // ------------------------------------------------------------
            // User-defined function / subroutine
            // ------------------------------------------------------------

            if (_functions.TryGetValue(
                    functionName,
                    out int userFunctionIndex))
            {
                if (callExpression.Arguments.Count != 0)
                    throw new InvalidOperationException(
                        $"Function '{functionName}' does not accept arguments.");

                Add(
                    OpCode.CallSubroutine,
                    userFunctionIndex);

                return VariableType.None;
            }

            // ------------------------------------------------------------
            // VM function
            // ------------------------------------------------------------

            int vmFunctionIndex =
                _vmFunctions.GetIndex(functionName);

            int argumentCount =
                callExpression.Arguments.Count;

            foreach (var argument in callExpression.Arguments)
            {
                Visit(argument);
            }

            VariableType returnType =
                _vmFunctions.GetReturnType(functionName);

            Add(
                OpCode.CallFunction,
                new FunctionCall(
                    (ushort)vmFunctionIndex,
                    (byte)argumentCount));

            if (returnType == VariableType.None)
            {
                Add(OpCode.Pop);
            }

            return returnType;
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
            foreach (var declaration in variableDeclaration.Declarations)
            {
                CompileVariableDeclarator(declaration, variableDeclaration.Kind);
            }

            return null;
        }

        private void CompileVariableDeclarator(VariableDeclarator variableDeclarator, VariableDeclarationKind varibleKind)
        {
            if (variableDeclarator.Id is not Identifier identifier)
                throw new InvalidOperationException(
                    "Only simple variable declarations are supported.");

            string name = identifier.Name;

            if (varibleKind == VariableDeclarationKind.Const)
            {
                if (variableDeclarator.Init == null)
                    throw new InvalidOperationException(
                        $"Constant '{name}' must have an initializer.");


                object value = GetConstantValue(variableDeclarator.Init);

                Debug.WriteLine("Adding const: " + name + " = " + value + " " + value.GetType().Name);
                _constants[name] = value;

                return;
            }

            Variable? existing = _variables
                .FirstOrDefault(x => x.Name == name);

            // No initializer.
            if (variableDeclarator.Init == null)
                return;

            bool isTypedArray =
                TryGetTypedArrayInitializer(
                    variableDeclarator.Init,
                    out VariableType typedArrayType,
                    out int typedArrayLength);

            VariableType type =
                isTypedArray
                    ? typedArrayType
                    : GetExpressionType(variableDeclarator.Init);

            // Existing variable.
            if (existing != null)
            {
                if (existing.Type != type)
                    throw new InvalidOperationException(
                        $"Variable '{name}' is {existing.Type}, " +
                        $"but initializer is {type}.");

                if (isTypedArray)
                {
                    if (!existing.IsArray)
                        throw new InvalidOperationException(
                            $"Variable '{name}' is not an array.");

                    if (existing.Length != typedArrayLength)
                        throw new InvalidOperationException(
                            $"Variable '{name}' already has length " +
                            $"{existing.Length}, requested {typedArrayLength}.");

                    // Memory is already allocated.
                    return;
                }

                if (varibleKind == VariableDeclarationKind.Using)
                {
                    int jumpIndex = _instructions.Count;

                    Add(OpCode.JumpIfInitialized, 0);

                    CompileExpression(
                        variableDeclarator.Init);

                    Store(existing);

                    int endIndex = _instructions.Count;

                    _instructions[jumpIndex] =
                        new Instruction(
                            OpCode.JumpIfInitialized,
                            endIndex);

                    return;
                }

                // var: always assign.
                if (variableDeclarator.Init is ArrayExpression array2)
                {
                    CompileArrayInitializer(
                        existing,
                        array2);
                }
                else
                {
                    CompileExpression(
                        variableDeclarator.Init);

                    Store(existing);
                }

                return;
            }

            // New variable.
            bool isArray = isTypedArray;
            int length = isTypedArray
                ? typedArrayLength
                : 1;

            int maxStringLength = 0;

            if (!isTypedArray &&
                variableDeclarator.Init is ArrayExpression array)
            {
                isArray = true;
                length = array.Elements.Count;

                if (type == VariableType.Str)
                {
                    foreach (var element in array.Elements)
                    {
                        if (element is Literal literal &&
                            literal.Value is string value)
                        {
                            maxStringLength =
                                Math.Max(
                                    maxStringLength,
                                    value.Length);
                        }
                    }
                }
            }
            else if (type == VariableType.Str &&
                     variableDeclarator.Init is Literal literal &&
                     literal.Value is string value)
            {
                maxStringLength = value.Length;
            }

            Variable variable =
                DeclareVariable(
                    name,
                    type,
                    varibleKind,
                    isArray,
                    length,
                    maxStringLength);

            if (isTypedArray)
                return;

            // let: declare the variable normally,
            // but skip its initializer once initialized.
            int letJumpIndex = -1;

            if (varibleKind == VariableDeclarationKind.Using)
            {
                letJumpIndex = _instructions.Count;

                Add(OpCode.JumpIfInitialized, 0);
            }

            if (isArray)
            {
                CompileArrayInitializer(
                    variable,
                    (ArrayExpression)variableDeclarator.Init);
            }
            else
            {
                CompileExpression(
                    variableDeclarator.Init);

                Store(variable);
            }

            if (letJumpIndex >= 0)
            {
                int endIndex = _instructions.Count;

                _instructions[letJumpIndex] =
                    new Instruction(
                        OpCode.JumpIfInitialized,
                        endIndex);
            }
        }

        private bool TryGetTypedArrayInitializer(
    Expression expression,
    out VariableType type,
    out int length)
        {
            type = VariableType.None;
            length = 0;

            if (expression is not MemberExpression member ||
                !member.Computed ||
                member.Object is not Identifier identifier ||
                member.Property is not Literal literal ||
                literal.Value is not double lengthValue)
            {
                return false;
            }

            type = identifier.Name switch
            {
                "int" => VariableType.Int,
                "float" => VariableType.Float,
                "byte" => VariableType.Byte,
                _ => VariableType.None
            };

            if (type == VariableType.None)
                return false;

            length = (int)lengthValue;

            if (length <= 0)
                throw new InvalidOperationException(
                    "Array length must be greater than zero.");

            return true;
        }

        private void CompileArrayInitializer(
                Variable variable,
                ArrayExpression array)
        {

            int elementSize =
                variable.GetElementSize();

            for (int i = 0; i < array.Elements.Count; i++)
            {
                int address =
                    variable.Address + (i * elementSize);

                // address
                Add(
                    OpCode.PushInt,
                    address);

                // value
                CompileExpression(
                    array.Elements[i]);

                // store
                Add(
                    variable.Type switch
                    {
                        VariableType.Int =>
                            OpCode.StoreIndirectInt,

                        VariableType.Float =>
                            OpCode.StoreIndirectFloat,

                        VariableType.Byte =>
                            OpCode.StoreIndirectByte,

                        VariableType.Bool =>
                            OpCode.StoreIndirectByte,

                        VariableType.Str =>
                            OpCode.StoreIndirectStr,

                        _ => throw new InvalidOperationException(
                            $"Unsupported array type: {variable.Type}")
                    });
            }
        }

        private VariableType GetValueType(object value)
        {
            return value switch
            {
                byte => VariableType.Byte,
                int => VariableType.Int,
                float => VariableType.Float,
                string => VariableType.Str,
                bool => VariableType.Bool,

                _ => throw new InvalidOperationException(
                    $"Unsupported constant type: {value.GetType().Name}")
            };
        }

        private OpCode GetValueOpcode(object value)
        {
            return value switch
            {
                byte => OpCode.PushByte,
                int => OpCode.PushInt,
                float => OpCode.PushFloat,
                
                _ => throw new InvalidOperationException(
                    $"Unsupported constant type: {value.GetType().Name}")
            };
        }

        private VariableType GetExpressionType(Expression expression)
        {
            if (expression is Literal literal)
            {
                return GetLiteralType(literal);
            }

            if (expression is Identifier identifier)
            {
                string name = identifier.Name;

                if (_constants.ContainsKey(name))
                {
                    return GetValueType(_constants[name]);
                }

                if (name.Length > 1 && name[0] == '$')
                {
                    return VariableType.Int;
                }

                Variable variable =
                    GetVariable(name);

                return variable.Type;
            }

            if (expression is BinaryExpression binary)
            {
                return GetBinaryExpressionType(binary);
            }

            if (expression is NonUpdateUnaryExpression unary)
            {
                return GetExpressionType(unary.Argument);
            }

            if (expression is MemberExpression memberExpression)
            {
                if (memberExpression.Object is Identifier identifier2 &&
                    memberExpression.Computed)
                {
                    Variable? variable =
                        _variables.FirstOrDefault(
                            x => x.Name == identifier2.Name);

                    if (variable == null)
                        throw new InvalidOperationException(
                            $"Variable '{identifier2.Name}' is not declared.");

                    if (!variable.IsArray)
                        throw new InvalidOperationException(
                            $"Variable '{identifier2.Name}' is not an array.");

                    return variable.Type;
                }
            }

            if (expression is ArrayExpression array)
            {
                if (array.Elements.Count == 0)
                    throw new InvalidOperationException(
                        "Array cannot be empty.");

                VariableType elementType =
                    GetExpressionType(array.Elements[0]);

                for (int i = 1; i < array.Elements.Count; i++)
                {
                    VariableType currentType =
                        GetExpressionType(array.Elements[i]);

                    if (currentType != elementType)
                    {
                        throw new InvalidOperationException(
                            $"Array element {i} is {currentType}, " +
                            $"but expected {elementType}.");
                    }
                }

                return elementType;
            }

            if (expression is ConditionalExpression conditional)
            {
                // Condition must be boolean
                VariableType testType = GetExpressionType(conditional.Test);

                if (testType != VariableType.Bool)
                    throw new InvalidOperationException(
                        $"Conditional test must be Bool, but got {testType}.");

                VariableType leftType = GetExpressionType(conditional.Consequent);
                VariableType rightType = GetExpressionType(conditional.Alternate);

                if (leftType == rightType)
                    return leftType;

                // Allow Int/Float promotion to Float
                if ((leftType == VariableType.Float && rightType == VariableType.Int) ||
                    (rightType == VariableType.Float && leftType == VariableType.Int))
                    return VariableType.Float;

                throw new InvalidOperationException(
                    $"Ternary branches must have the same type, got {leftType} and {rightType}.");
            }

            if (expression is CallExpression call)
            {
                if (call.Callee is not Identifier functionIdentifier)
                    throw new InvalidOperationException(
                        "Only named function calls are supported.");

                return _vmFunctions.GetReturnType(functionIdentifier.Name);
            }

            throw new InvalidOperationException(
                $"Unsupported expression type: {expression.GetType().Name}");
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

            if (literal.Value is string value)
            {
                string text = GetText(literal.Range);

                if (text.Length >= 2 &&
                    text[0] == '\'' &&
                    text[^1] == '\'')
                {
                    if (value.Length != 1)
                        throw new InvalidOperationException(
                            "Character literal must contain exactly one character.");

                    return VariableType.Byte;
                }

                return VariableType.Str;
            }

            if (literal.Value is int ||
                literal.Value is long)
                return VariableType.Int;

            if (literal.Value is double)
            {
                string text = GetText(literal.Range);
                if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
    literal.Value is double hexValue &&
    hexValue >= 0 &&
    hexValue <= 255)
                {
                    return VariableType.Byte;
                }

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
                    Add(
                        OpCode.PushInt,
                        Convert.ToInt32(literal.Value));
                    break;

                case VariableType.Float:
                    Add(
                        OpCode.PushFloat,
                        Convert.ToSingle(literal.Value));
                    break;

                case VariableType.Bool:
                    Add(
                        OpCode.PushByte,
                        (bool)literal.Value ? 1 : 0);
                    break;

                case VariableType.Byte:
                    {
                        string text = GetText(literal.Range);

                        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            Add(
                                OpCode.PushByte,
                                Convert.ToByte(literal.Value));
                        }
                        else
                        {
                            // Character literal
                            Add(
                                OpCode.PushByte,
                                (byte)((string)literal.Value!)[0]);
                        }

                        break;
                    }

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
                Add(GetValueOpcode(_constants[name]), _constants[name]);
                return null;
            }
            else if (name.Length > 1 && name[0] == '$')
            {
                Add(OpCode.PushInt, GetVariable(name.AsSpan(1).ToString()).Address);
                return null;
            }
            

            Variable variable = GetVariable(name);

            Load(variable);

            return null;
        }

        protected override object? VisitUnaryExpression(
    UnaryExpression node)
        {
            if (node is UpdateExpression update)
            {
                if (update.Argument is not Identifier identifier)
                {
                    throw new InvalidOperationException(
                        "Increment and decrement operators are only supported for variables.");
                }

                Variable variable =
                    GetVariable(identifier.Name);

                // Load current value
                Load(variable);

                // +1 / -1
                Add(OpCode.PushInt, 1);

                Add(
                    update.Operator == Operator.Increment
                        ? OpCode.Add
                        : OpCode.Subtract);

                // Store result
                Store(variable);

                return variable.Type;
            }

            // Unary minus
            Add(OpCode.PushInt, 0);

            Visit(node.Argument);

            Add(OpCode.Subtract);

            return null;
        }

        protected override object? VisitAssignmentExpression(
    AssignmentExpression assignment)
        {
            // Whole array assignment
            if (assignment.Left is Identifier identifier &&
                assignment.Operator == Operator.Assignment &&
                assignment.Right is ArrayExpression array)
            {
                Variable variable =
                    GetVariable(identifier.Name);

                if (!variable.IsArray)
                {
                    throw new InvalidOperationException(
                        $"Variable '{variable.Name}' is not an array.");
                }

                if (array.Elements.Count != variable.Length)
                {
                    throw new InvalidOperationException(
                        $"Array '{variable.Name}' has length {variable.Length}, " +
                        $"but assignment contains {array.Elements.Count} elements.");
                }

                VariableType arrayType =
                    GetExpressionType(array);

                if (arrayType != variable.Type)
                {
                    throw new InvalidOperationException(
                        $"Array '{variable.Name}' is {variable.Type}[], " +
                        $"but assigned value is {arrayType}[].");
                }

                CompileArrayInitializer(
                    variable,
                    array);

                return variable.Type;
            }

            // Normal variable assignment
            if (assignment.Left is Identifier identifier2)
            {
                Variable variable =
                    GetVariable(identifier2.Name);

                // Normal =
                if (assignment.Operator == Operator.Assignment)
                {
                    CompileExpression(assignment.Right);
                    Store(variable);

                    return variable.Type;
                }

                // Compound assignment
                if (assignment.Operator == Operator.AdditionAssignment ||
                    assignment.Operator == Operator.SubtractionAssignment)
                {
                    Load(variable);

                    CompileExpression(assignment.Right);

                    Add(
                        assignment.Operator ==
                            Operator.AdditionAssignment
                            ? OpCode.Add
                            : OpCode.Subtract);

                    Store(variable);

                    return variable.Type;
                }

                throw new InvalidOperationException(
                    $"Assignment operator '{assignment.Operator}' is not supported.");
            }

            // Array element assignment
            if (assignment.Left is MemberExpression member)
            {
                if (!member.Computed ||
                    member.Object is not Identifier identifier3)
                {
                    throw new InvalidOperationException(
                        "Only array indexing is supported.");
                }

                Variable variable =
                    GetVariable(identifier3.Name);

                if (!variable.IsArray)
                    throw new InvalidOperationException(
                        $"Variable '{variable.Name}' is not an array.");

                VariableType indexType =
                    GetExpressionType(member.Property);

                if (indexType != VariableType.Int)
                    throw new InvalidOperationException(
                        $"Array index must be Int, but got {indexType}.");

                // Address
                Add(
                    OpCode.PushInt,
                    variable.Address);

                // Index
                CompileExpression(member.Property);

                // index * element size
                Add(
                    OpCode.PushInt,
                    variable.GetElementSize());

                Add(OpCode.Multiply);

                // base + offset
                Add(OpCode.Add);

                // Value
                CompileExpression(assignment.Right);

                // Store
                Add(
                    variable.Type switch
                    {
                        VariableType.Int =>
                            OpCode.StoreIndirectInt,

                        VariableType.Float =>
                            OpCode.StoreIndirectFloat,

                        VariableType.Byte =>
                            OpCode.StoreIndirectByte,

                        VariableType.Bool =>
                            OpCode.StoreIndirectByte,

                        VariableType.Str =>
                            OpCode.StoreIndirectStr,

                        _ => throw new InvalidOperationException(
                            $"Unsupported array type: {variable.Type}")
                    });

                return variable.Type;
            }

            throw new InvalidOperationException(
                "Only simple variable or array element assignment is supported.");
        }


        protected override object? VisitMemberExpression(
    MemberExpression member)
        {
            if (!member.Computed ||
                member.Object is not Identifier identifier)
            {
                throw new InvalidOperationException(
                    "Only array indexing is supported.");
            }

            Variable variable =
                GetVariable(identifier.Name);

            if (!variable.IsArray)
                throw new InvalidOperationException(
                    $"Variable '{variable.Name}' is not an array.");

            // The index expression must produce an integer.
            VariableType indexType =
                GetExpressionType(member.Property);

            if (indexType != VariableType.Int)
                throw new InvalidOperationException(
                    $"Array index must be Int, but got {indexType}.");

            // Base address.
            Add(
                OpCode.PushInt,
                variable.Address);

            // Index.
            CompileExpression(member.Property);

            // index * elementSize
            Add(
                OpCode.PushInt,
                variable.GetElementSize());

            Add(OpCode.Multiply);

            // base + offset
            Add(OpCode.Add);

            // Load element.
            Add(
                variable.Type switch
                {
                    VariableType.Int =>
                        OpCode.LoadIndirectInt,

                    VariableType.Float =>
                        OpCode.LoadIndirectFloat,

                    VariableType.Byte =>
                        OpCode.LoadIndirectByte,

                    VariableType.Bool =>
                        OpCode.LoadIndirectByte,

                    VariableType.Str =>
                        OpCode.LoadIndirectStr,

                    _ => throw new InvalidOperationException(
                        $"Unsupported array type: {variable.Type}")
                });

            return variable.Type;
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
        VariableDeclarationKind kind,
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
                kind,
                isArray,
                length,
                maxStringLength);

            _variables.Add(variable);
            UpdateAddresses();

            return variable;
        }

        private object GetConstantValue(Expression expression)
        {
            if (expression is not Literal literal)
                throw new InvalidOperationException(
                    "Constants must be initialized with literal values.");

            VariableType literalType = GetLiteralType(literal);

            return literalType switch
            {
                VariableType.Int => Convert.ToInt32(literal.Value),
                VariableType.Float => Convert.ToSingle(literal.Value),
                VariableType.Bool => Convert.ToBoolean(literal.Value),
                VariableType.Byte => Convert.ToByte(literal.Value),
                VariableType.Str => Convert.ToString(literal.Value)
                    ?? string.Empty,

                _ => throw new InvalidOperationException(
                    $"Unsupported constant type: {literalType}.")
            };
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

using System.Globalization;
using NTOSCompiler.Compiler;

namespace NTOSCompiler;

public sealed class Parser
{
    private readonly List<Token> _tokens;
    private int _position;

    private VMFunctions _vmFunctions;
    private string _currentFunction = null;

    public Parser(List<Token> tokens, VMFunctions vmFunctions = null)
    {
        _tokens = tokens;
        _vmFunctions = vmFunctions;
    }

    public BytecodeProgram Compile()
    {
        var program = new BytecodeProgram();

        while (!Check(TokenKind.Eof))
        {
            bool requiresSemicolon = CompileStatement(program);

            if (requiresSemicolon)
            {
                if (!Match(TokenKind.Semicolon))
                    throw Error("Expected ';'.");
            }
        }

        return program;
    }

    private bool CompileStatement(BytecodeProgram program)
    {
        if (Check(TokenKind.IntType) ||
            Check(TokenKind.FloatType) ||
            Check(TokenKind.BoolType) ||
            Check(TokenKind.StringType))
        {
            CompileVariableDeclaration(program);
            return true;
        }

        if (Match(TokenKind.If))
        {
            CompileIf(program);
            return false;
        }

        if (Match(TokenKind.While))
        {
            CompileWhile(program);
            return false;
        }

        if (Match(TokenKind.Function))
        {
            CompileFunctionDeclaration(program);
            return false;
        }

        if (Match(TokenKind.Return))
        {
            return CompileReturnStatement(program);
        }

        // x = expression
        if (Check(TokenKind.Identifier) &&
            Peek(1).Kind == TokenKind.Equals)
        {
            string name = Advance().Text;
            Advance(); // '='

            VariableDefinition variable =
                program.GetVariable(
                    name,
                    _currentFunction,
                    VariableType.Void);

            VariableType expressionType =
                CompileExpression(program);

            if (expressionType != variable.Type)
            {
                throw Error(
                    $"Cannot assign {expressionType} to {variable.Type} variable '{name}'.");
            }

            program.Instructions.Add(
                new Instruction(
                    GetStoreOpcode(variable.Type),
                    variable.Address));

            return true;
        }

        // expression
        CompileExpression(program);

        program.Instructions.Add(
            new Instruction(OpCode.Pop));

        return true;
    }

    private bool CompileReturnStatement(BytecodeProgram program)
    {
        VariableType expressionType =
            CompileExpression(program);

        if (_currentFunction == null)
            throw Error("'return' can only be used inside a function.");

        FunctionDefinition function =
            program.Functions.First(f => f.Name == _currentFunction);

        if (expressionType != function.ReturnType)
        {
            throw Error(
                $"Cannot return {expressionType} from {function.ReturnType} function.");
        }

        program.Instructions.Add(
            new Instruction(OpCode.Return));

        return true;
    }

    private void CompileVariableDeclaration(
    BytecodeProgram program)
    {
        VariableType declaredType =
            ParseVariableType();

        string name =
            Consume(
                TokenKind.Identifier,
                "Expected variable name.").Text;

        Consume(
            TokenKind.Equals,
            "Expected '='.");

        VariableType expressionType =
            CompileExpression(program);

        if (declaredType != expressionType)
        {
            throw Error(
                $"Cannot assign {expressionType} to {declaredType} variable '{name}'.");
        }

        VariableDefinition variable =
            program.GetVariable(
                name,
                _currentFunction,
                declaredType);

        program.Instructions.Add(
            new Instruction(
                GetStoreOpcode(variable.Type),
                variable.Address));
    }

    private static OpCode GetLoadOpcode(VariableType type)
    {
        return type switch
        {
            VariableType.Int => OpCode.LoadInt,
            VariableType.Float => OpCode.LoadFloat,

            _ => throw new NotSupportedException(
                $"Load is not supported for variable type {type}.")
        };
    }

    private static OpCode GetStoreOpcode(VariableType type)
    {
        return type switch
        {
            VariableType.Int => OpCode.StoreInt,
            VariableType.Float => OpCode.StoreFloat,

            _ => throw new NotSupportedException(
                $"Store is not supported for variable type {type}.")
        };
    }

    private void CompileIf(BytecodeProgram program)
    {
        Consume(
            TokenKind.LParen,
            "Expected '(' after 'if'.");

        VariableType conditionType =
            CompileExpression(program);

        if (conditionType != VariableType.Bool)
        {
            throw Error(
                "Expected boolean expression in 'if' condition.");
        }

        Consume(
            TokenKind.RParen,
            "Expected ')' after condition.");

        Consume(
            TokenKind.LBrace,
            "Expected '{'.");

        int jumpIfFalseIndex =
            program.Instructions.Count;

        program.Instructions.Add(
            new Instruction(
                OpCode.JumpIfFalse,
                -1));

        while (!Check(TokenKind.RBrace) &&
               !Check(TokenKind.Eof))
        {
            bool requiresSemicolon =
                CompileStatement(program);

            if (requiresSemicolon)
            {
                if (!Match(TokenKind.Semicolon))
                    throw Error("Expected ';'.");
            }
        }

        Consume(
            TokenKind.RBrace,
            "Expected '}'.");

        // No else
        if (!Match(TokenKind.Else))
        {
            int endIndex =
                program.Instructions.Count;

            program.Instructions[jumpIfFalseIndex] =
                new Instruction(
                    OpCode.JumpIfFalse,
                    endIndex);

            return;
        }

        // We have an else.
        int jumpIndex =
            program.Instructions.Count;

        program.Instructions.Add(
            new Instruction(
                OpCode.Jump,
                -1));

        // False condition should jump here.
        int elseIndex =
            program.Instructions.Count;

        program.Instructions[jumpIfFalseIndex] =
            new Instruction(
                OpCode.JumpIfFalse,
                elseIndex);

        Consume(
            TokenKind.LBrace,
            "Expected '{' after 'else'.");

        while (!Check(TokenKind.RBrace) &&
               !Check(TokenKind.Eof))
        {
            bool requiresSemicolon =
                CompileStatement(program);

            if (requiresSemicolon)
            {
                if (!Match(TokenKind.Semicolon))
                    throw Error("Expected ';'.");
            }
        }

        Consume(
            TokenKind.RBrace,
            "Expected '}'.");

        int end =
            program.Instructions.Count;

        program.Instructions[jumpIndex] =
            new Instruction(
                OpCode.Jump,
                end);
    }

    private void CompileWhile(BytecodeProgram program)
    {
        Consume(
            TokenKind.LParen,
            "Expected '(' after 'while'.");

        int loopStart =
            program.Instructions.Count;

        VariableType conditionType =
            CompileExpression(program);

        if (conditionType != VariableType.Bool)
        {
            throw Error(
                "Expected boolean expression in 'while' condition.");
        }

        Consume(
            TokenKind.RParen,
            "Expected ')' after condition.");

        Consume(
            TokenKind.LBrace,
            "Expected '{'.");

        int jumpIfFalseIndex =
            program.Instructions.Count;

        program.Instructions.Add(
            new Instruction(
                OpCode.JumpIfFalse,
                -1));

        while (!Check(TokenKind.RBrace) &&
               !Check(TokenKind.Eof))
        {
            bool requiresSemicolon =
                CompileStatement(program);

            if (requiresSemicolon)
            {
                if (!Match(TokenKind.Semicolon))
                    throw Error("Expected ';'.");
            }
        }

        Consume(
            TokenKind.RBrace,
            "Expected '}'.");

        program.Instructions.Add(
            new Instruction(
                OpCode.Jump,
                loopStart));

        int endIndex =
            program.Instructions.Count;

        program.Instructions[jumpIfFalseIndex] =
            new Instruction(
                OpCode.JumpIfFalse,
                endIndex);
    }

    private VariableType CompileExpression(
        BytecodeProgram program)
    {
        return CompileOr(program);
    }

    private VariableType CompileOr(
        BytecodeProgram program)
    {
        VariableType leftType =
            CompileAnd(program);

        while (Match(TokenKind.OrOr))
        {
            VariableType rightType =
                CompileAnd(program);

            if (leftType != VariableType.Bool ||
                rightType != VariableType.Bool)
            {
                throw Error(
                    "Logical OR requires boolean operands.");
            }

            program.Instructions.Add(
                new Instruction(OpCode.Or));

            leftType = VariableType.Bool;
        }

        return leftType;
    }

    private VariableType CompileAnd(
        BytecodeProgram program)
    {
        VariableType leftType =
            CompileEquality(program);

        while (Match(TokenKind.AndAnd))
        {
            VariableType rightType =
                CompileEquality(program);

            if (leftType != VariableType.Bool ||
                rightType != VariableType.Bool)
            {
                throw Error(
                    "Logical AND requires boolean operands.");
            }

            program.Instructions.Add(
                new Instruction(OpCode.And));

            leftType = VariableType.Bool;
        }

        return leftType;
    }

    private VariableType CompileEquality(
        BytecodeProgram program)
    {
        VariableType leftType =
            CompileAdditive(program);

        while (Check(TokenKind.EqualEqual) ||
               Check(TokenKind.NotEqual) ||
               Check(TokenKind.Less) ||
               Check(TokenKind.Greater) ||
               Check(TokenKind.LessEqual) ||
               Check(TokenKind.GreaterEqual))
        {
            TokenKind op =
                Advance().Kind;

            VariableType rightType =
                CompileAdditive(program);

            ValidateComparisonTypes(
                leftType,
                rightType);

            program.Instructions.Add(
                new Instruction(
                    op switch
                    {
                        TokenKind.EqualEqual =>
                            OpCode.Equal,

                        TokenKind.NotEqual =>
                            OpCode.NotEqual,

                        TokenKind.Less =>
                            OpCode.Less,

                        TokenKind.Greater =>
                            OpCode.Greater,

                        TokenKind.LessEqual =>
                            OpCode.LessEqual,

                        TokenKind.GreaterEqual =>
                            OpCode.GreaterEqual,

                        _ => throw new InvalidOperationException()
                    }));

            leftType = VariableType.Bool;
        }

        return leftType;
    }

    private VariableType CompileAdditive(
        BytecodeProgram program)
    {
        VariableType leftType =
            CompileTerm(program);

        while (Check(TokenKind.Plus) ||
               Check(TokenKind.Minus))
        {
            TokenKind op =
                Advance().Kind;

            VariableType rightType =
                CompileTerm(program);

            leftType = GetNumericResultType(
                leftType,
                rightType);

            program.Instructions.Add(
                new Instruction(
                    op == TokenKind.Plus
                        ? OpCode.Add
                        : OpCode.Subtract));
        }

        return leftType;
    }

    private VariableType CompileTerm(
        BytecodeProgram program)
    {
        VariableType leftType =
            CompileFactor(program);

        while (Check(TokenKind.Star) ||
               Check(TokenKind.Slash) ||
               Check(TokenKind.Percent))
        {
            TokenKind op =
                Advance().Kind;

            VariableType rightType =
                CompileFactor(program);

            leftType = GetNumericResultType(
                leftType,
                rightType);

            program.Instructions.Add(
                new Instruction(
                    op switch
                    {
                        TokenKind.Star =>
                            OpCode.Multiply,

                        TokenKind.Slash =>
                            OpCode.Divide,

                        TokenKind.Percent =>
                            OpCode.Modulo,

                        _ => throw new InvalidOperationException()
                    }));
        }

        return leftType;
    }

    private VariableType CompileFactor(
        BytecodeProgram program)
    {
        if (Match(TokenKind.Not))
        {
            VariableType operandType =
                CompileFactor(program);

            if (operandType != VariableType.Bool)
            {
                throw Error(
                    "Logical NOT requires a boolean operand.");
            }

            program.Instructions.Add(
                new Instruction(OpCode.Not));

            return VariableType.Bool;
        }

        if (Check(TokenKind.Identifier) &&
            Peek(1).Kind == TokenKind.LParen)
        {
            return CompileFunctionCall(program);
        }

        if (Match(TokenKind.Int))
        {
            program.Instructions.Add(
                new Instruction(
                    OpCode.PushInt,
                    int.Parse(
                        Previous().Text,
                        CultureInfo.InvariantCulture)));

            return VariableType.Int;
        }

        if (Match(TokenKind.Float))
        {
            program.Instructions.Add(
                new Instruction(
                    OpCode.PushFloat,
                    float.Parse(
                        Previous().Text,
                        CultureInfo.InvariantCulture)));

            return VariableType.Float;
        }

        if (Match(TokenKind.String))
        {
            program.Instructions.Add(
                new Instruction(
                    OpCode.PushString,
                    Previous().Text));

            return VariableType.String;
        }

        if (Match(TokenKind.True))
        {
            program.Instructions.Add(
                new Instruction(
                    OpCode.PushBool,
                    true));

            return VariableType.Bool;
        }

        if (Match(TokenKind.False))
        {
            program.Instructions.Add(
                new Instruction(
                    OpCode.PushBool,
                    false));

            return VariableType.Bool;
        }

        if (Match(TokenKind.Identifier))
        {
            string name =
                Previous().Text;

            VariableDefinition variable =
                program.GetVariable(
                    name,
                    _currentFunction,
                    VariableType.Void);

            program.Instructions.Add(
                new Instruction(
                    GetLoadOpcode(variable.Type),
                    variable.Address));

            return variable.Type;
        }

        if (Match(TokenKind.LParen))
        {
            VariableType type =
                CompileExpression(program);

            Consume(
                TokenKind.RParen,
                "Expected ')'.");

            return type;
        }

        throw Error(
            $"Unexpected token '{Current().Text}'.");
    }

    private static VariableType GetNumericResultType(
        VariableType left,
        VariableType right)
    {
        bool leftIsNumeric =
            left == VariableType.Int ||
            left == VariableType.Float;

        bool rightIsNumeric =
            right == VariableType.Int ||
            right == VariableType.Float;

        if (!leftIsNumeric || !rightIsNumeric)
        {
            throw new InvalidOperationException(
                $"Numeric operation cannot be performed between {left} and {right}.");
        }

        if (left == VariableType.Float ||
            right == VariableType.Float)
        {
            return VariableType.Float;
        }

        return VariableType.Int;
    }

    private static void ValidateComparisonTypes(
        VariableType left,
        VariableType right)
    {
        bool leftIsNumeric =
            left == VariableType.Int ||
            left == VariableType.Float;

        bool rightIsNumeric =
            right == VariableType.Int ||
            right == VariableType.Float;

        if (leftIsNumeric && rightIsNumeric)
            return;

        if (left == VariableType.Bool &&
            right == VariableType.Bool)
        {
            return;
        }

        if (left == VariableType.String &&
            right == VariableType.String)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Cannot compare {left} and {right}.");
    }

    private VariableType CompileFunctionDeclaration(
    BytecodeProgram program)
    {
        

        string name =
            Consume(
                TokenKind.Identifier,
                "Expected function name.").Text;

        Consume(
            TokenKind.LParen,
            "Expected '('.");

        var parameters = new List<ParameterDefinition>();

        if (!Check(TokenKind.RParen))
        {
            do
            {
                VariableType parameterType = ParseVariableType();

                string parameterName =
                    Consume(
                        TokenKind.Identifier,
                        "Expected parameter name.").Text;

                parameters.Add(
                    new ParameterDefinition(
                        parameterName,
                        parameterType));
            }
            while (Match(TokenKind.Comma));
        }

        Consume(
            TokenKind.RParen,
            "Expected ')'.");

        VariableType returnType =
            ParseVariableType();

        Consume(
            TokenKind.LBrace,
            "Expected '{'.");

        var function =
            new FunctionDefinition(
                name,
                returnType,
                parameters.ToArray(),
                program.Instructions.Count);

        program.Functions.Add(function);

        _currentFunction = function.Name;

        foreach (var parameter in parameters)
        {
            program.GetVariable(
                parameter.Name,
                _currentFunction,
                parameter.Type);
        }

        while (!Check(TokenKind.RBrace) &&
               !Check(TokenKind.Eof))
        {
            bool requiresSemicolon =
                CompileStatement(program);

            if (requiresSemicolon)
            {
                if (!Match(TokenKind.Semicolon))
                    throw Error("Expected ';'.");
            }
        }

        Consume(
            TokenKind.RBrace,
            "Expected '}'.");



        _currentFunction = null;

        // Function declarations are statements and do not
        // produce a value.
        return VariableType.Void;
    }

    private VariableType CompileFunctionCall(
    BytecodeProgram program)
    {
        string functionName =
            Advance().Text;

        FunctionDefinition? userFunction =
            program.Functions.FirstOrDefault(
                f => f.Name == functionName);

        Consume(
            TokenKind.LParen,
            "Expected '('.");

        int argumentCount = 0;

        if (!Check(TokenKind.RParen))
        {
            do
            {
                CompileExpression(program);
                argumentCount++;
            }
            while (Match(TokenKind.Comma));
        }

        Consume(
            TokenKind.RParen,
            "Expected ')'.");

        if (userFunction != null)
        {
            if (argumentCount != userFunction.Parameters.Length)
            {
                throw Error(
                    $"Function '{functionName}' expects " +
                    $"{userFunction.Parameters.Length} arguments, " +
                    $"but {argumentCount} were provided.");
            }

            var parameterAddresses =
                new int[userFunction.Parameters.Length];

            for (int i = 0;
                 i < userFunction.Parameters.Length;
                 i++)
            {
                var parameter =
                    userFunction.Parameters[i];

                var variable =
                    program.GetVariable(
                        parameter.Name,
                        userFunction.Name,
                        parameter.Type);

                parameterAddresses[i] =
                    variable.Address;
            }

            program.Instructions.Add(
                new Instruction(
                    OpCode.CallUserFunction,
                    new UserFunctionCall(
                        userFunction.EntryPoint,
                        parameterAddresses)));

            return userFunction.ReturnType;
        }

        int functionIndex =
            _vmFunctions?.GetIndex(functionName) ?? -1;

        program.Instructions.Add(
            new Instruction(
                OpCode.CallFunction,
                new FunctionCall(
                    functionIndex,
                    argumentCount)));

        return _vmFunctions?.GetReturnType(functionIndex)
            ?? VariableType.Void;
    }



    private VariableType ParseVariableType()
    {
        if (Match(TokenKind.IntType))
            return VariableType.Int;

        if (Match(TokenKind.FloatType))
            return VariableType.Float;

        if (Match(TokenKind.BoolType))
            return VariableType.Int;

        if (Match(TokenKind.StringType))
            return VariableType.String;

        throw Error("Expected variable type.");
    }

    private bool Match(TokenKind kind)
    {
        if (!Check(kind))
            return false;

        Advance();
        return true;
    }

    private Token Consume(
        TokenKind kind,
        string message)
    {
        if (!Check(kind))
            throw Error(message);

        return Advance();
    }

    private bool Check(TokenKind kind)
        => Current().Kind == kind;

    private Token Advance()
    {
        if (_position < _tokens.Count)
            _position++;

        return Previous();
    }

    private Token Current()
        => _tokens[
            Math.Min(
                _position,
                _tokens.Count - 1)];

    private Token Previous()
        => _tokens[_position - 1];

    private Token Peek(int offset)
        => _tokens[
            Math.Min(
                _position + offset,
                _tokens.Count - 1)];

    private Exception Error(string message)
        => new Exception(
            $"{message} Position: {Current().Position}.");
}
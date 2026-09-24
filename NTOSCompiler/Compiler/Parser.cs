using Microsoft.VisualBasic;
using NTOSCompiler.Compiler;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NTOSCompiler;

public sealed class Parser
{
    private readonly List<Token> _tokens;
    private int _position;
    private readonly VMFunctions _vmFunctions;
    private Dictionary<string, byte> _constants;

    private readonly Dictionary<string, int> _subroutines = new();

    private readonly List<(int InstructionIndex, string Name)> _unresolvedSubroutineCalls = new();

    public Parser(List<Token> tokens, VMFunctions vmFunctions, Dictionary<string, byte>? constants)
    {
        _vmFunctions = vmFunctions ?? new VMFunctions();
        _constants = constants ?? new();
        _tokens = tokens;
    }

    public BytecodeProgram Compile(BytecodeProgram program = null)
    {
        _subroutines.Clear();
        _unresolvedSubroutineCalls.Clear();
        program = program ?? new BytecodeProgram();
        program.Instructions.Clear();

        while (!Check(TokenKind.Eof))
        {
            bool requiresSemicolon = CompileStatement(program);

            if (requiresSemicolon)
            {
                if (!Match(TokenKind.Semicolon))
                    throw Error("Expected ';'.");
            }
        }

        program.Instructions.Add(
            new Instruction(OpCode.End));

        ResolveSubroutineCalls(program);
        program.UpdateBytecode();

        return program;
    }

    private bool CompileStatement(BytecodeProgram program)
    {
        if (Match(TokenKind.Call))
        {
            CompileSubroutineCall(program);
            return true;
        }

        if (Match(TokenKind.Delay))
        {
            CompileDelay(program);
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

        if (Check(TokenKind.IntType) ||
            Check(TokenKind.FloatType) ||
            Check(TokenKind.BoolType) ||
            Check(TokenKind.StrType))
        {
            CompileDeclaration(program);
            return true;
        }

        if (Check(TokenKind.Identifier) &&
            Peek(1).Kind == TokenKind.LBrace)
        {
            CompileSubroutine(program);
            return false;
        }

        // x = expression
        if (Check(TokenKind.Identifier) &&
            Peek(1).Kind == TokenKind.Assign)
        {
            string name = Advance().Text;
            Advance(); // '='

            Variable variable = program.GetVariable(name);

            VariableType expressionType =
                CompileExpression(program);

            if (expressionType != variable.Type)
            {
                throw Error(
                    $"Cannot assign {expressionType} to variable " +
                    $"'{name}' of type {variable.Type}.");
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

    private void CompileDelay(BytecodeProgram program)
    {
        Token value = Advance();

        if (value.Kind != TokenKind.Float &&
            value.Kind != TokenKind.Int)
        {
            throw Error(
                "Expected number after 'delay'.");
        }

        float seconds = float.Parse(
            value.Text,
            System.Globalization.CultureInfo.InvariantCulture);

        if (seconds < 0)
        {
            throw Error(
                "Delay cannot be negative.");
        }

        program.Instructions.Add(
            new Instruction(
                OpCode.Delay,
                seconds));
    }

    private void CompileSubroutine(BytecodeProgram program)
    {
        string name = Advance().Text;

        Consume(
            TokenKind.LBrace,
            "Expected '{' after subroutine name.");

        if (_subroutines.ContainsKey(name))
        {
            throw Error(
                $"Subroutine '{name}' is already declared.");
        }

        // Skip subroutine during normal execution.
        int jumpIndex = program.Instructions.Count;

        program.Instructions.Add(
            new Instruction(
                OpCode.Jump,
                -1));

        // First instruction of the actual subroutine body.
        _subroutines[name] =
            program.Instructions.Count;

        while (!Check(TokenKind.RBrace) &&
               !Check(TokenKind.Eof))
        {
            bool requiresSemicolon =
                CompileStatement(program);

            if (requiresSemicolon)
            {
                Consume(
                    TokenKind.Semicolon,
                    "Expected ';'.");
            }
        }

        Consume(
            TokenKind.RBrace,
            "Expected '}' after subroutine.");

        // Automatically return when the subroutine reaches its end.
        program.Instructions.Add(
            new Instruction(OpCode.Return));

        // Skip the entire subroutine during normal execution.
        int endIndex =
            program.Instructions.Count;

        program.Instructions[jumpIndex] =
            new Instruction(
                OpCode.Jump,
                endIndex);
    }

    private void CompileSubroutineCall(BytecodeProgram program)
    {
        Token name = Consume(
            TokenKind.Identifier,
            "Expected subroutine name.");

        int instructionIndex = program.Instructions.Count;

        program.Instructions.Add(
            new Instruction(
                OpCode.CallSubroutine,
                -1));

        _unresolvedSubroutineCalls.Add(
            (instructionIndex, name.Text));
    }

    private void ResolveSubroutineCalls(BytecodeProgram program)
    {
        foreach (var (instructionIndex, name)
            in _unresolvedSubroutineCalls)
        {
            if (!_subroutines.TryGetValue(
                    name,
                    out int targetInstructionIndex))
            {
                throw new Exception(
                    $"Unknown subroutine '{name}'.");
            }

            program.Instructions[instructionIndex] =
                new Instruction(
                    OpCode.CallSubroutine,
                    targetInstructionIndex);
        }
    }

    private void CompileDeclaration(BytecodeProgram program)
    {
        VariableType type;

        int maxLength = 0;

        if (Match(TokenKind.IntType))
        {
            type = VariableType.Int;
        }
        else if (Match(TokenKind.FloatType))
        {
            type = VariableType.Float;
        }
        else if (Match(TokenKind.BoolType))
        {
            type = VariableType.Bool;
        }
        else if (Match(TokenKind.StrType))
        {
            type = VariableType.Str;

            if (Match(TokenKind.LBracket))
            {
                maxLength = ParseArrayLength();
                Consume(
                    TokenKind.RBracket,
                    "Expected ']' after string length.");
            }
        }
        else
        {
            throw Error("Expected type.");
        }

        Token name = Consume(
            TokenKind.Identifier,
            "Expected variable name.");

        bool isArray = false;
        int length = 1;

        if (Match(TokenKind.LBracket))
        {
            length = ParseArrayLength();

            Consume(
                TokenKind.RBracket,
                "Expected ']' after array length.");

            isArray = true;
        }

        var variable = program.DeclareVariable(
            name.Text,
            type,
            isArray,
            length,
            maxLength);

        if (Match(TokenKind.Assign))
        {
            if (isArray)
            {
                throw Error(
                    "Array initializers are not implemented yet.");
            }

            var expressionType = CompileExpression(program);

            if (expressionType != type)
            {
                throw Error(
                    $"Cannot assign {expressionType} to variable " +
                    $"'{name.Text}' of type {type}.");
            }

            program.Instructions.Add(
                new Instruction(
                    GetStoreOpcode(type),
                    variable.Address));
        }
    }

    private int ParseArrayLength()
    {
        Token length = Consume(
            TokenKind.Int,
            "Expected constant length.");

        int value = int.Parse(length.Text);

        if (value <= 0)
            throw Error("Length must be greater than zero.");

        return value;
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
                "If condition must be of type Bool.");
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

        program.Instructions.Add(
            new Instruction(OpCode.Checkpoint));

        int loopStart =
            program.Instructions.Count;

        VariableType conditionType =
            CompileExpression(program);

        if (conditionType != VariableType.Bool)
        {
            throw Error(
                "While condition must be of type Bool.");
        }

        Consume(
            TokenKind.RParen,
            "Expected ')' after 'while' condition.");

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

    private VariableType CompileExpression(BytecodeProgram program)
    {
        return CompileOr(program);
    }

    private VariableType CompileOr(
        BytecodeProgram program)
    {
        VariableType type =
            CompileAnd(program);

        while (Match(TokenKind.OrOr))
        {
            if (type != VariableType.Bool)
            {
                throw Error(
                    "Left operand of '||' must be Bool.");
            }

            VariableType rightType =
                CompileAnd(program);

            if (rightType != VariableType.Bool)
            {
                throw Error(
                    "Right operand of '||' must be Bool.");
            }

            program.Instructions.Add(
                new Instruction(OpCode.Or));

            type = VariableType.Bool;
        }

        return type;
    }

    private VariableType CompileAnd(
        BytecodeProgram program)
    {
        VariableType type =
            CompileEquality(program);

        while (Match(TokenKind.AndAnd))
        {
            if (type != VariableType.Bool)
            {
                throw Error(
                    "Left operand of '&&' must be Bool.");
            }

            VariableType rightType =
                CompileEquality(program);

            if (rightType != VariableType.Bool)
            {
                throw Error(
                    "Right operand of '&&' must be Bool.");
            }

            program.Instructions.Add(
                new Instruction(OpCode.And));

            type = VariableType.Bool;
        }

        return type;
    }

    private VariableType CompileEquality(
        BytecodeProgram program)
    {
        VariableType type =
            CompileAdditive(program);

        while (Check(TokenKind.Equals) ||
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

            if (type != rightType)
            {
                throw Error(
                    $"Cannot compare {type} with {rightType}.");
            }

            program.Instructions.Add(
                new Instruction(
                    op switch
                    {
                        TokenKind.Equals =>
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

            type = VariableType.Bool;
        }

        return type;
    }

    private VariableType CompileAdditive(
        BytecodeProgram program)
    {
        VariableType type =
            CompileTerm(program);

        while (Check(TokenKind.Plus) ||
               Check(TokenKind.Minus))
        {
            TokenKind op =
                Advance().Kind;

            VariableType rightType =
                CompileTerm(program);

            if (!IsNumeric(type) ||
                !IsNumeric(rightType))
            {
                throw Error(
                    $"Operator '{op}' requires numeric operands.");
            }

            if (type != rightType)
            {
                throw Error(
                    $"Cannot combine {type} with {rightType} yet. " +
                    "Numeric conversion is not implemented yet.");
            }

            program.Instructions.Add(
                new Instruction(
                    op == TokenKind.Plus
                        ? OpCode.Add
                        : OpCode.Subtract));
        }

        return type;
    }

    private VariableType CompileTerm(
        BytecodeProgram program)
    {
        VariableType type =
            CompileFactor(program);

        while (Check(TokenKind.Star) ||
               Check(TokenKind.Slash) ||
               Check(TokenKind.Percent))
        {
            TokenKind op =
                Advance().Kind;

            VariableType rightType =
                CompileFactor(program);

            if (!IsNumeric(type) ||
                !IsNumeric(rightType))
            {
                throw Error(
                    $"Operator '{op}' requires numeric operands.");
            }

            if (type != rightType)
            {
                throw Error(
                    $"Cannot combine {type} with {rightType} yet. " +
                    "Numeric conversion is not implemented yet.");
            }

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

        return type;
    }

    private VariableType CompileFactor(
        BytecodeProgram program)
    {
        if (Match(TokenKind.Not))
        {
            VariableType type =
                CompileFactor(program);

            if (type != VariableType.Bool)
            {
                throw Error(
                    "Operator '!' requires a Bool operand.");
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
                    int.Parse(Previous().Text)));

            return VariableType.Int;
        }

        if (Match(TokenKind.Float))
        {
            program.Instructions.Add(
                new Instruction(
                    OpCode.PushFloat,
                    float.Parse(
                        Previous().Text,
                        System.Globalization.CultureInfo.InvariantCulture)));

            return VariableType.Float;
        }

        if (Match(TokenKind.Str))
        {
            program.Instructions.Add(
                new Instruction(
                    OpCode.PushStr,
                    Previous().Text));

            return VariableType.Str;
        }

        if (Match(TokenKind.True))
        {
            program.Instructions.Add(
                new Instruction(
                    OpCode.PushByte,
                    1));

            return VariableType.Bool;
        }

        if (Match(TokenKind.False))
        {
            program.Instructions.Add(
                new Instruction(
                    OpCode.PushByte,
                    0));

            return VariableType.Bool;
        }

        if (Match(TokenKind.Read))
        {
            return CompileRead(program);
        }

        if (Match(TokenKind.Identifier))
        {
            string name = Previous().Text;

            if (_constants.TryGetValue(name, out byte constant))
            {
                program.Instructions.Add(
                    new Instruction(
                        OpCode.PushByte,
                        constant));

                return VariableType.Byte;
            }

            Variable variable = program.GetVariable(name);

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

    private VariableType CompileRead(BytecodeProgram program)
    {
        Token variableToken = Consume(
            TokenKind.Identifier,
            "Expected variable name after 'read'.");

        string name = variableToken.Text;

        Variable variable = program.GetVariable(name);

        program.Instructions.Add(
            new Instruction(
                GetLoadOpcode(variable.Type),
                variable.Address));

        return variable.Type;
    }

    private VariableType CompileFunctionCall(
        BytecodeProgram program)
    {
        string functionName =
            Advance().Text;

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

        if (_vmFunctions == null)
        {
            throw new InvalidOperationException(
                "VMFunctions is required to compile function calls.");
        }

        program.Instructions.Add(
            new Instruction(
                OpCode.CallFunction,
                new FunctionCall(
                    (ushort)_vmFunctions.GetIndex(functionName),
                    (byte)argumentCount)));

        // VM function return types are not represented yet.
        return VariableType.None;
    }

    private static bool IsNumeric(
        VariableType type)
    {
        return type == VariableType.Int ||
               type == VariableType.Float;
    }

    private static OpCode GetLoadOpcode(
        VariableType type)
    {
        return type switch
        {
            VariableType.Int =>
                OpCode.LoadInt,

            VariableType.Float =>
                OpCode.LoadFloat,

            VariableType.Bool =>
                OpCode.LoadByte,

            VariableType.Byte =>
                OpCode.LoadByte,

            VariableType.Str =>
                OpCode.LoadStr,

            _ => throw new InvalidOperationException(
                $"Cannot load variable of type {type}.")
        };
    }

    private static OpCode GetStoreOpcode(
        VariableType type)
    {
        return type switch
        {
            VariableType.Int =>
                OpCode.StoreInt,

            VariableType.Float =>
                OpCode.StoreFloat,

            VariableType.Bool =>
                OpCode.StoreByte,

            VariableType.Str =>
                OpCode.StoreStr,

            _ => throw new InvalidOperationException(
                $"Cannot store variable of type {type}.")
        };
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

using NTOSCompiler.Compiler;
using System.Xml.Linq;

namespace NTOSCompiler;

public sealed class Parser
{
    private readonly List<Token> _tokens;
    private int _position;
    private readonly VMFunctions _vmFunctions;

    public Parser(List<Token> tokens, VMFunctions vmFunctions)
    {
        _vmFunctions = vmFunctions;
        _tokens = tokens;
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

        program.Instructions.Add(
                new Instruction(OpCode.End));
        program.UpdateBytecode();
        return program;
    }

    private bool CompileStatement(BytecodeProgram program)
    {
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

        // x = expression
        if (Check(TokenKind.Identifier) &&
            Peek(1).Kind == TokenKind.Equals)
        {
            string name = Advance().Text;
            Advance(); // '='

            CompileExpression(program);

            int address = program.GetAddress(name, 4);

            program.Instructions.Add(
                new Instruction(
                    OpCode.StoreVariable,
                    address));

            return true;
        }

        // expression
        CompileExpression(program);

        program.Instructions.Add(
            new Instruction(OpCode.Pop));
        return true;
    }

    private void CompileIf(BytecodeProgram program)
    {
        Consume(
            TokenKind.LParen,
            "Expected '(' after 'if'.");

        CompileExpression(program);

        Consume(
            TokenKind.RParen,
            "Expected ')' after condition.");

        Consume(
            TokenKind.LBrace,
            "Expected '{'.");

        int jumpIfFalseIndex = program.Instructions.Count;

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
            int endIndex = program.Instructions.Count;

            program.Instructions[jumpIfFalseIndex] =
                new Instruction(
                    OpCode.JumpIfFalse,
                    endIndex);

            return;
        }

        // We have an else.
        int jumpIndex = program.Instructions.Count;

        program.Instructions.Add(
            new Instruction(
                OpCode.Jump,
                -1));

        // False condition should jump here.
        int elseIndex = program.Instructions.Count;

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

        int end = program.Instructions.Count;

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

        int loopStart = program.Instructions.Count;

        CompileExpression(program);

        Consume(
            TokenKind.RParen,
            "Expected ')' after condition.");

        Consume(
            TokenKind.LBrace,
            "Expected '{'.");

        int jumpIfFalseIndex = program.Instructions.Count;

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

        int endIndex = program.Instructions.Count;

        program.Instructions[jumpIfFalseIndex] =
            new Instruction(
                OpCode.JumpIfFalse,
                endIndex);
    }

    private void CompileExpression(BytecodeProgram program)
    {
        CompileOr(program);
    }

    private void CompileOr(BytecodeProgram program)
    {
        CompileAnd(program);

        while (Match(TokenKind.OrOr))
        {
            CompileAnd(program);

            program.Instructions.Add(
                new Instruction(OpCode.Or));
        }
    }

    private void CompileAnd(BytecodeProgram program)
    {
        CompileEquality(program);

        while (Match(TokenKind.AndAnd))
        {
            CompileEquality(program);

            program.Instructions.Add(
                new Instruction(OpCode.And));
        }
    }

    private void CompileEquality(BytecodeProgram program)
    {
        CompileAdditive(program);

        while (Check(TokenKind.EqualEqual) ||
               Check(TokenKind.NotEqual) ||
               Check(TokenKind.Less) ||
               Check(TokenKind.Greater) ||
               Check(TokenKind.LessEqual) ||
               Check(TokenKind.GreaterEqual))
        {
            TokenKind op = Advance().Kind;

            CompileAdditive(program);

            program.Instructions.Add(
                new Instruction(
                    op switch
                    {
                        TokenKind.EqualEqual => OpCode.Equal,
                        TokenKind.NotEqual => OpCode.NotEqual,
                        TokenKind.Less => OpCode.Less,
                        TokenKind.Greater => OpCode.Greater,
                        TokenKind.LessEqual => OpCode.LessEqual,
                        TokenKind.GreaterEqual => OpCode.GreaterEqual,
                        _ => throw new InvalidOperationException()
                    }));
        }
    }

    private void CompileAdditive(BytecodeProgram program)
    {
        CompileTerm(program);

        while (Check(TokenKind.Plus) ||
               Check(TokenKind.Minus))
        {
            TokenKind op = Advance().Kind;

            CompileTerm(program);

            program.Instructions.Add(
                new Instruction(
                    op == TokenKind.Plus
                        ? OpCode.Add
                        : OpCode.Subtract));
        }
    }

    private void CompileTerm(BytecodeProgram program)
    {
        CompileFactor(program);

        while (Check(TokenKind.Star) ||
               Check(TokenKind.Slash) ||
               Check(TokenKind.Percent))
        {
            TokenKind op = Advance().Kind;

            CompileFactor(program);

            program.Instructions.Add(
                new Instruction(
                    op switch
                    {
                        TokenKind.Star => OpCode.Multiply,
                        TokenKind.Slash => OpCode.Divide,
                        TokenKind.Percent => OpCode.Modulo,
                        _ => throw new InvalidOperationException()
                    }));
        }
    }

    private void CompileFactor(BytecodeProgram program)
    {
        if (Match(TokenKind.Not))
        {
            CompileFactor(program);

            program.Instructions.Add(
                new Instruction(OpCode.Not));

            return;
        }

        if (Check(TokenKind.Identifier) &&
            Peek(1).Kind == TokenKind.LParen)
        {
            CompileFunctionCall(program);
            return;
        }

        if (Match(TokenKind.Integer))
        {
            program.Instructions.Add(
                new Instruction(
                    OpCode.PushInt,
                    long.Parse(Previous().Text)));

            return;
        }

        if (Match(TokenKind.Float))
        {
            program.Instructions.Add(
                new Instruction(
                    OpCode.PushFloat,
                    double.Parse(
                        Previous().Text,
                        System.Globalization.CultureInfo.InvariantCulture)));

            return;
        }

        if (Match(TokenKind.String))
        {
            program.Instructions.Add(
                new Instruction(
                    OpCode.PushString,
                    Previous().Text));

            return;
        }

        if (Match(TokenKind.True))
        {
            program.Instructions.Add(
                new Instruction(
                    OpCode.PushBool,
                    true));

            return;
        }

        if (Match(TokenKind.False))
        {
            program.Instructions.Add(
                new Instruction(
                    OpCode.PushBool,
                    false));

            return;
        }

        if (Match(TokenKind.Identifier))
        {
            string name = Previous().Text;

            int address = program.GetAddress(name, 4);

            program.Instructions.Add(
                new Instruction(
                    OpCode.LoadVariable,
                    address));

            return;
        }

        if (Match(TokenKind.LParen))
        {
            CompileExpression(program);

            Consume(
                TokenKind.RParen,
                "Expected ')'.");

            return;
        }

        throw Error(
            $"Unexpected token '{Current().Text}'.");
    }

    private void CompileFunctionCall(BytecodeProgram program)
    {
        string functionName = Advance().Text;

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
            throw new InvalidOperationException(
                "VMFunctions is required to compile function calls.");

        program.Instructions.Add(
            new Instruction(
                OpCode.CallFunction,
                new FunctionCall(
                    (ushort)_vmFunctions?.GetIndex(functionName),
                    (byte)argumentCount)));
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
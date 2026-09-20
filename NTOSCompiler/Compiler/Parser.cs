namespace NTOSCompiler;

public sealed class Parser
{
    private readonly List<Token> _tokens;
    private int _position;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public BytecodeProgram Compile()
    {
        var program = new BytecodeProgram();

        while (!Check(TokenKind.Eof))
        {
            CompileStatement(program);

            if (Match(TokenKind.Semicolon))
                continue;

            if (!Check(TokenKind.Eof))
                throw Error("Expected ';'.");
        }

        return program;
    }

    private void CompileStatement(BytecodeProgram program)
    {
        // x = expression
        if (Check(TokenKind.Identifier) &&
            Peek(1).Kind == TokenKind.Equals)
        {
            string name = Advance().Text;
            Advance(); // '='

            CompileExpression(program);

            program.Instructions.Add(
                new Instruction(OpCode.StoreVariable, name));

            return;
        }

        // expression
        CompileExpression(program);
        program.Instructions.Add(new Instruction(OpCode.Pop));
    }

    private void CompileExpression(BytecodeProgram program)
    {
        CompileTerm(program);

        while (Check(TokenKind.Plus) || Check(TokenKind.Minus))
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
                new Instruction(op switch
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
            program.Instructions.Add(
                new Instruction(
                    OpCode.LoadVariable,
                    Previous().Text));

            return;
        }

        if (Match(TokenKind.LParen))
        {
            CompileExpression(program);
            Consume(TokenKind.RParen, "Expected ')'.");
            return;
        }

        throw Error($"Unexpected token '{Current().Text}'.");
    }

    private void CompileFunctionCall(BytecodeProgram program)
    {
        string functionName = Advance().Text;

        Consume(TokenKind.LParen, "Expected '('.");

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

        Consume(TokenKind.RParen, "Expected ')'.");

        program.Instructions.Add(
            new Instruction(
                OpCode.CallFunction,
                new FunctionCall(functionName, argumentCount)));
    }

    private bool Match(TokenKind kind)
    {
        if (!Check(kind))
            return false;

        Advance();
        return true;
    }

    private Token Consume(TokenKind kind, string message)
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
        => _tokens[Math.Min(_position, _tokens.Count - 1)];

    private Token Previous()
        => _tokens[_position - 1];

    private Token Peek(int offset)
        => _tokens[Math.Min(_position + offset, _tokens.Count - 1)];

    private Exception Error(string message)
        => new Exception($"{message} Position: {Current().Position}.");
}

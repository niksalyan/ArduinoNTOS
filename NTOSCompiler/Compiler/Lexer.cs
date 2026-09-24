namespace NTOSCompiler;

public sealed class Lexer
{
    private readonly string _source;
    private int _position;

    public Lexer(string source)
    {
        _source = source;
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while (_position < _source.Length)
        {
            char c = _source[_position];

            if (char.IsWhiteSpace(c))
            {
                _position++;
                continue;
            }

            int start = _position;

            if (char.IsLetter(c) || c == '_')
            {
                _position++;

                while (_position < _source.Length &&
                       (char.IsLetterOrDigit(_source[_position]) ||
                        _source[_position] == '_'))
                {
                    _position++;
                }

                string text = _source[start.._position];

                TokenKind wordKind = text switch
                {
                    "int" => TokenKind.IntType,
                    "float" => TokenKind.FloatType,
                    "bool" => TokenKind.BoolType,
                    "str" => TokenKind.StrType,

                    "true" => TokenKind.True,
                    "false" => TokenKind.False,
                    "if" => TokenKind.If,
                    "else" => TokenKind.Else,
                    "while" => TokenKind.While,
                    "call" => TokenKind.Call,
                    "delay" => TokenKind.Delay,
                    "read" => TokenKind.Read,

                    _ => TokenKind.Identifier
                };

                tokens.Add(new Token(wordKind, text, start));
                continue;
            }

            if (char.IsDigit(c))
            {
                bool isFloat = false;

                _position++;

                while (_position < _source.Length &&
                       char.IsDigit(_source[_position]))
                {
                    _position++;
                }

                if (_position < _source.Length &&
                    _source[_position] == '.' &&
                    _position + 1 < _source.Length &&
                    char.IsDigit(_source[_position + 1]))
                {
                    isFloat = true;
                    _position++;

                    while (_position < _source.Length &&
                           char.IsDigit(_source[_position]))
                    {
                        _position++;
                    }
                }

                tokens.Add(new Token(
                    isFloat ? TokenKind.Float : TokenKind.Int,
                    _source[start.._position],
                    start));

                continue;
            }

            if (c == '"')
            {
                _position++;

                while (_position < _source.Length &&
                       _source[_position] != '"')
                {
                    _position++;
                }

                if (_position >= _source.Length)
                    throw new Exception(
                        $"Unterminated string at position {start}.");

                _position++;

                tokens.Add(new Token(
                    TokenKind.Str,
                    _source[(start + 1)..(_position - 1)],
                    start));

                continue;
            }

            if (c == '=')
            {
                _position++;

                if (_position < _source.Length && _source[_position] == '=')
                {
                    _position++;
                    tokens.Add(new Token(
                        TokenKind.Equals,
                        "==",
                        start));
                }
                else
                {
                    tokens.Add(new Token(
                        TokenKind.Assign,
                        "=",
                        start));
                }

                continue;
            }

            if (c == '!')
            {
                _position++;

                if (_position < _source.Length &&
                    _source[_position] == '=')
                {
                    _position++;

                    tokens.Add(new Token(
                        TokenKind.NotEqual,
                        "!=",
                        start));
                }
                else
                {
                    tokens.Add(new Token(
                        TokenKind.Not,
                        "!",
                        start));
                }

                continue;
            }

            if (c == '&')
            {
                _position++;

                if (_position < _source.Length &&
                    _source[_position] == '&')
                {
                    _position++;

                    tokens.Add(new Token(
                        TokenKind.AndAnd,
                        "&&",
                        start));

                    continue;
                }

                throw new Exception(
                    $"Unexpected character '&' at position {start}.");
            }

            if (c == '|')
            {
                _position++;

                if (_position < _source.Length &&
                    _source[_position] == '|')
                {
                    _position++;

                    tokens.Add(new Token(
                        TokenKind.OrOr,
                        "||",
                        start));

                    continue;
                }

                throw new Exception(
                    $"Unexpected character '|' at position {start}.");
            }

            if (c == '<')
            {
                _position++;

                if (_position < _source.Length &&
                    _source[_position] == '=')
                {
                    _position++;

                    tokens.Add(new Token(
                        TokenKind.LessEqual,
                        "<=",
                        start));
                }
                else
                {
                    tokens.Add(new Token(
                        TokenKind.Less,
                        "<",
                        start));
                }

                continue;
            }

            if (c == '>')
            {
                _position++;

                if (_position < _source.Length &&
                    _source[_position] == '=')
                {
                    _position++;

                    tokens.Add(new Token(
                        TokenKind.GreaterEqual,
                        ">=",
                        start));
                }
                else
                {
                    tokens.Add(new Token(
                        TokenKind.Greater,
                        ">",
                        start));
                }

                continue;
            }

            if (c == '/')
            {
                // Single-line comment
                if (_position + 1 < _source.Length &&
                    _source[_position + 1] == '/')
                {
                    _position += 2;

                    while (_position < _source.Length &&
                           _source[_position] != '\n')
                    {
                        _position++;
                    }

                    continue;
                }

                // Normal division operator
                _position++;

                tokens.Add(new Token(
                    TokenKind.Slash,
                    "/",
                    start));

                continue;
            }

            TokenKind kind = c switch
            {
                '+' => TokenKind.Plus,
                '-' => TokenKind.Minus,
                '*' => TokenKind.Star,
                '%' => TokenKind.Percent,

                '(' => TokenKind.LParen,
                ')' => TokenKind.RParen,

                '{' => TokenKind.LBrace,
                '}' => TokenKind.RBrace,

                '[' => TokenKind.LBracket,
                ']' => TokenKind.RBracket,

                ',' => TokenKind.Comma,
                ';' => TokenKind.Semicolon,

                _ => throw new Exception(
                    $"Unexpected character '{c}' at position {start}.")
            };

            _position++;

            tokens.Add(new Token(
                kind,
                c.ToString(),
                start));
        }

        tokens.Add(new Token(
            TokenKind.Eof,
            string.Empty,
            _position));

        return tokens;
    }
}
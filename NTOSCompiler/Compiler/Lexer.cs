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
                       (char.IsLetterOrDigit(_source[_position]) || _source[_position] == '_'))
                {
                    _position++;
                }

                tokens.Add(new Token(
                    TokenKind.Identifier,
                    _source[start.._position],
                    start));

                continue;
            }

            if (char.IsDigit(c))
            {
                bool isFloat = false;

                _position++;

                while (_position < _source.Length && char.IsDigit(_source[_position]))
                    _position++;

                if (_position < _source.Length &&
                    _source[_position] == '.' &&
                    _position + 1 < _source.Length &&
                    char.IsDigit(_source[_position + 1]))
                {
                    isFloat = true;
                    _position++;

                    while (_position < _source.Length && char.IsDigit(_source[_position]))
                        _position++;
                }

                tokens.Add(new Token(
                    isFloat ? TokenKind.Float : TokenKind.Integer,
                    _source[start.._position],
                    start));

                continue;
            }

            if (c == '"')
            {
                _position++;

                while (_position < _source.Length && _source[_position] != '"')
                    _position++;

                if (_position >= _source.Length)
                    throw new Exception($"Unterminated string at position {start}.");

                _position++;

                tokens.Add(new Token(
                    TokenKind.String,
                    _source[(start + 1)..(_position - 1)],
                    start));

                continue;
            }

            TokenKind kind = c switch
            {
                '+' => TokenKind.Plus,
                '-' => TokenKind.Minus,
                '*' => TokenKind.Star,
                '/' => TokenKind.Slash,
                '%' => TokenKind.Percent,
                '=' => TokenKind.Equals,
                '(' => TokenKind.LParen,
                ')' => TokenKind.RParen,
                ',' => TokenKind.Comma,
                ';' => TokenKind.Semicolon,
                _ => throw new Exception($"Unexpected character '{c}' at position {start}.")
            };

            _position++;
            tokens.Add(new Token(kind, c.ToString(), start));
        }

        tokens.Add(new Token(TokenKind.Eof, string.Empty, _position));

        return tokens;
    }
}

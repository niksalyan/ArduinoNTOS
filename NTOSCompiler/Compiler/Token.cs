namespace NTOSCompiler;

public enum TokenKind
{
    Eof,

    Identifier,
    Integer,
    Float,
    String,

    Plus,
    Minus,
    Star,
    Slash,
    Percent,

    

    True,
    False,
    Equals,
    EqualEqual,
    NotEqual,

    Less,
    Greater,
    LessEqual,
    GreaterEqual,

    LParen,
    RParen,
    Comma,
    Semicolon,


}

public readonly record struct Token(
    TokenKind Kind,
    string Text,
    int Position);

public readonly record struct FunctionCall(
    string Name,
    int ArgumentCount);
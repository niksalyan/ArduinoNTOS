namespace NTOSCompiler;

public enum TokenKind
{
    Eof,

    IntType,
    FloatType,
    BoolType,

    ByteType,
    StrType,

    Identifier,

    Assign,
    Int,
    Float,

    Bool,
    Str,

    Plus,
    Minus,
    Star,
    Slash,
    Percent,

    If,
    Else,

    While,
    True,
    False,
    Equals,
    NotEqual,

    Less,
    Greater,
    LessEqual,
    GreaterEqual,

    AndAnd,
    OrOr,
    Not,

    LParen,
    RParen,

    LBrace,
    RBrace,

    LBracket,
    RBracket,
    Comma,
    Semicolon,


}

public readonly record struct Token(
    TokenKind Kind,
    string Text,
    int Position);

public readonly record struct FunctionCall(
    ushort Index,
    byte ArgumentCount);
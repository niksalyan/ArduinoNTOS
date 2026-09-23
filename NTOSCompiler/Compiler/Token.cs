namespace NTOSCompiler;

public enum TokenKind
{
    Eof,

    IntType,
    FloatType,
    BoolType,
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
    EqualEqual,
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
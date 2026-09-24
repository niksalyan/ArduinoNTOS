namespace NTOSCompiler;

public enum TokenKind
{
    Eof,


    IntType,
    FloatType,
    BoolType,

    ByteType,

    CharType,
    StrType,

    Identifier,

    CharLiteral,

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

    Read,

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

    Call,

    Delay


}

public readonly record struct Token(
    TokenKind Kind,
    string Text,
    int Position);

public readonly record struct FunctionCall(
    ushort Index,
    byte ArgumentCount);
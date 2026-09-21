namespace NTOSCompiler;

public enum TokenKind
{
    Eof,

    IntType,
    FloatType,
    BoolType,
    StringType,

    Identifier,
    Int,
    Float,
    String,

    Bool,

    Plus,
    Minus,
    Star,
    Slash,
    Percent,

    If,
    Else,

    While,

    Function,
    Return,
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
    int FunctionIndex,
    int ArgumentCount);

public sealed class UserFunctionCall
{
    public int EntryPoint { get; }
    public int[] ParameterAddresses { get; }

    public UserFunctionCall(
        int entryPoint,
        int[] parameterAddresses)
    {
        EntryPoint = entryPoint;
        ParameterAddresses = parameterAddresses;
    }
}
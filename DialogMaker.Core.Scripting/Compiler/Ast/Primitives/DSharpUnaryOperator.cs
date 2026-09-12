using DialogMaker.Core.Scripting.CodeAnalyzer;
using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    [GenerateInformation(EnumValuesInformation.OnlyValues)]
    public enum DSharpUnaryOperator
    {
        Not = DSharpTokenType.Not,
        Minus = DSharpTokenType.Minus,
        Increment = DSharpTokenType.Increment,
        Decrement = DSharpTokenType.Decrement
    }
}

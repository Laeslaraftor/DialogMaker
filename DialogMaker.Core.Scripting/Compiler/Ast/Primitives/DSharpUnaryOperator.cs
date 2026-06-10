using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    public enum DSharpUnaryOperator
    {
        Not = DSharpTokenType.Not,
        Minus = DSharpTokenType.Minus,
        Increment = DSharpTokenType.Increment,
        Decrement = DSharpTokenType.Decrement
    }
}

using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    public enum DSharpLiteralType
    {
        Null = DSharpTokenType.Null,
        Number = DSharpTokenType.NumberLiteral,
        Bool = DSharpTokenType.Bool,
        String = DSharpTokenType.StringLiteral
    }
}

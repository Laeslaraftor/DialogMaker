using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    public enum DialogScriptLiteralType
    {
        Null = DialogScriptTokenType.Null,
        Number = DialogScriptTokenType.NumberLiteral,
        Bool = DialogScriptTokenType.Bool,
        String = DialogScriptTokenType.StringLiteral
    }
}

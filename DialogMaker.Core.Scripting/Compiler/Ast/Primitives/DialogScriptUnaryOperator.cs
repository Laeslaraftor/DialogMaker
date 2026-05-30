using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    public enum DialogScriptUnaryOperator
    {
        Not = DialogScriptTokenType.Not,
        Plus = DialogScriptTokenType.Plus,
        Minus = DialogScriptTokenType.Minus,
        Increment = DialogScriptTokenType.Increment,
        Decrement = DialogScriptTokenType.Decrement
    }
}

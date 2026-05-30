using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    public enum DialogScriptBinaryOperator
    {
        Plus = DialogScriptTokenType.Plus,
        Minus = DialogScriptTokenType.Minus,
        Multiply = DialogScriptTokenType.Multiply,
        Divide = DialogScriptTokenType.Divide,
        Mod = DialogScriptTokenType.Mod,
        LogicalOr = DialogScriptTokenType.Or,
        LogicalAnd = DialogScriptTokenType.And,
        LogicalNot = DialogScriptTokenType.Not,
    }
}

using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    public enum DialogScriptAssignmentOperator
    {
        Assign = DialogScriptTokenType.Assign,
        PlusAssign = DialogScriptTokenType.PlusAssign,
        MinusAssign = DialogScriptTokenType.MinusAssign,
        DivideAssign = DialogScriptTokenType.DivideAssign,
        MultiplyAssign = DialogScriptTokenType.MultiplyAssign
    }
}

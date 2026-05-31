using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    public enum DSharpAssignmentOperator
    {
        Assign = DSharpTokenType.Assign,
        PlusAssign = DSharpTokenType.PlusAssign,
        MinusAssign = DSharpTokenType.MinusAssign,
        DivideAssign = DSharpTokenType.DivideAssign,
        MultiplyAssign = DSharpTokenType.MultiplyAssign
    }
}

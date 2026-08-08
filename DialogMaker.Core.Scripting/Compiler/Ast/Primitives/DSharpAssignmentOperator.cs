using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    public enum DSharpAssignmentOperator
    {
        Assign = DSharpTokenType.Assign,
        AssignIfNull = DSharpTokenType.AssignIfNull,
        PlusAssign = DSharpTokenType.PlusAssign,
        MinusAssign = DSharpTokenType.MinusAssign,
        DivideAssign = DSharpTokenType.DivideAssign,
        MultiplyAssign = DSharpTokenType.MultiplyAssign,
        ModAssign = DSharpTokenType.ModAssign,
        AndAssign = DSharpTokenType.AndAssign,
        OrAssign = DSharpTokenType.OrAssign,
        XorAssign = DSharpTokenType.XorAssign,
    }
}

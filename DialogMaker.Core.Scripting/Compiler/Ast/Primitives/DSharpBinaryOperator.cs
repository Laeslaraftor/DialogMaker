using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    public enum DSharpBinaryOperator
    {
        Plus = DSharpTokenType.Plus,
        Minus = DSharpTokenType.Minus,
        Multiply = DSharpTokenType.Multiply,
        Divide = DSharpTokenType.Divide,
        Mod = DSharpTokenType.Mod,
        LogicalOr = DSharpTokenType.Or,
        LogicalAnd = DSharpTokenType.And,
        LogicalEquals = DSharpTokenType.Equal,
        LogicalNotEquals = DSharpTokenType.NotEqual,
        LogicalLess = DSharpTokenType.Less,
        LogicalLessOrEquals = DSharpTokenType.LessEqual,
        LogicalGreater = DSharpTokenType.Greater,
        LogicalGreaterOrEquals = DSharpTokenType.GreaterEqual,
    }
}

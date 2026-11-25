using DialogMaker.Core.Scripting.CodeAnalyzer;
using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Binary operator
    /// </summary>
    public enum DSharpBinaryOperator
    {
        /// <summary>
        /// Plus operator. Add right value to left
        /// </summary>
        [Name("Add")]
        Plus = DSharpTokenType.Plus,
        /// <summary>
        /// Minus operator. Subtract right value from left
        /// </summary>
        [Name("Subtract")]
        Minus = DSharpTokenType.Minus,
        /// <summary>
        /// Multiply operator. Multiply left value by right
        /// </summary>
        Multiply = DSharpTokenType.Multiply,
        /// <summary>
        /// Divide operator. Divide left value by right
        /// </summary>
        Divide = DSharpTokenType.Divide,
        /// <summary>
        /// %
        /// </summary>
        Mod = DSharpTokenType.Mod,
        /// <summary>
        /// <<
        /// </summary>
        ShiftLeft = DSharpTokenType.ShiftLeft,
        /// <summary>
        /// >>
        /// </summary>
        ShiftRight = DSharpTokenType.ShiftRight,
        /// <summary>
        /// &
        /// </summary>
        And = DSharpTokenType.And,
        /// <summary>
        /// |
        /// </summary>
        Or = DSharpTokenType.Or,
        /// <summary>
        /// ^
        /// </summary>
        [Name("Xor")]
        LogicalXor = DSharpTokenType.Xor,
        /// <summary>
        /// ||
        /// </summary>
        [Ignore] // ignore for generating instructions executor because they equals to OR
        LogicalOr = DSharpTokenType.LogicalOr,
        /// <summary>
        /// &&
        /// </summary>
        [Ignore] // ignore for generating instructions executor because they equals to AND
        LogicalAnd = DSharpTokenType.LogicalAnd,
        /// <summary>
        /// ==
        /// </summary>
        [Name("Equals")]
        LogicalEquals = DSharpTokenType.Equal,
        /// <summary>
        /// !=
        /// </summary>
        [Name("NotEquals")]
        LogicalNotEquals = DSharpTokenType.NotEqual,
        /// <summary>
        /// <
        /// </summary>
        [Name("Less")]
        LogicalLess = DSharpTokenType.Less,
        /// <summary>
        /// <=
        /// </summary>
        [Name("LessOrEqual")]
        LogicalLessOrEquals = DSharpTokenType.LessEqual,
        /// <summary>
        /// >
        /// </summary>
        [Name("Greater")]
        LogicalGreater = DSharpTokenType.Greater,
        /// <summary>
        /// >=
        /// </summary>
        [Name("GreaterOrEqual")]
        LogicalGreaterOrEquals = DSharpTokenType.GreaterEqual,
    }
}

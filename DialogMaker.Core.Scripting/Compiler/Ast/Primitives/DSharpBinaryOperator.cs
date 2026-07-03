using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    /// <summary>
    /// Binary operator
    /// </summary>
    public enum DSharpBinaryOperator
    {
        /// <summary>
        /// Plus operator. Add right value to left
        /// </summary>
        Plus = DSharpTokenType.Plus,
        /// <summary>
        /// Minus operator. Subtract right value from left
        /// </summary>
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
        /// 
        /// </summary>
        Mod = DSharpTokenType.Mod,
        /// <summary>
        /// 
        /// </summary>
        ShiftLeft = DSharpTokenType.ShiftLeft,
        /// <summary>
        /// 
        /// </summary>
        ShiftRight = DSharpTokenType.ShiftRight,
        /// <summary>
        /// 
        /// </summary>
        And = DSharpTokenType.And,
        /// <summary>
        /// 
        /// </summary>
        Or = DSharpTokenType.Or,
        /// <summary>
        /// 
        /// </summary>
        LogicalXor = DSharpTokenType.Xor,
        /// <summary>
        /// 
        /// </summary>
        LogicalOr = DSharpTokenType.LogicalOr,
        /// <summary>
        /// 
        /// </summary>
        LogicalAnd = DSharpTokenType.LogicalAnd,
        /// <summary>
        /// 
        /// </summary>
        LogicalEquals = DSharpTokenType.Equal,
        /// <summary>
        /// 
        /// </summary>
        LogicalNotEquals = DSharpTokenType.NotEqual,
        /// <summary>
        /// 
        /// </summary>
        LogicalLess = DSharpTokenType.Less,
        /// <summary>
        /// 
        /// </summary>
        LogicalLessOrEquals = DSharpTokenType.LessEqual,
        /// <summary>
        /// 
        /// </summary>
        LogicalGreater = DSharpTokenType.Greater,
        /// <summary>
        /// 
        /// </summary>
        LogicalGreaterOrEquals = DSharpTokenType.GreaterEqual,
    }
}

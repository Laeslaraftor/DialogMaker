using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    /// <summary>
    /// Literal type
    /// </summary>
    public enum DSharpLiteralType
    {
        /// <summary>
        /// Null value
        /// </summary>
        Null = DSharpTokenType.Null,
        /// <summary>
        /// Number value
        /// </summary>
        Number = DSharpTokenType.NumberLiteral,
        /// <summary>
        /// Boolean value
        /// </summary>
        Bool = DSharpTokenType.Bool,
        /// <summary>
        /// String value
        /// </summary>
        String = DSharpTokenType.StringLiteral,
        /// <summary>
        /// Char value
        /// </summary>
        Char = DSharpTokenType.CharLiteral
    }
}

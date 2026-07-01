using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Type of cast operator
    /// </summary>
    public enum DSharpOperatorType
    {
        /// <summary>
        /// Unary operator. 
        /// This operator applies to 1 object
        /// </summary>
        Unary,
        /// <summary>
        /// Binary operator.
        /// This operator requiers 2 objects
        /// </summary>
        Binary,
        /// <summary>
        /// Implicit cast operator.
        /// Operators with this type will be used automatically
        /// </summary>
        Implicit = DSharpTokenType.Implicit,
        /// <summary>
        /// Explicit cast operator.
        /// To use operators with this type user should specify cast operation
        /// </summary>
        Explicit = DSharpTokenType.Explicit
    }
}

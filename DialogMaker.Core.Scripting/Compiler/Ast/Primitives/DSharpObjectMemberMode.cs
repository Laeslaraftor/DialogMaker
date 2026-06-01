using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    /// <summary>
    /// Member mode
    /// </summary>
    public enum DSharpObjectMemberMode
    {
        /// <summary>
        /// Default member
        /// </summary>
        Default,
        /// <summary>
        /// Virtual member
        /// </summary>
        Virtual,
        /// <summary>
        /// Abstract member
        /// </summary>
        Abstract
    }
}

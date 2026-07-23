using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// D# property accessor
    /// </summary>
    public enum DSharpPropertyAccessor
    {
        /// <summary>
        /// Get accessor
        /// </summary>
        Getter = DSharpTokenType.Get,
        /// <summary>
        /// Set accessor
        /// </summary>
        Setter = DSharpTokenType.Set,
    }
}

using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    /// <summary>
    /// Access modifier of object type or it's member
    /// </summary>
    public enum DSharpAccessModifier
    {
        /// <summary>
        /// Public access modifier. Object type or member can be accessed without limitations 
        /// </summary>
        Public = DSharpTokenType.Public,
        /// <summary>
        /// Private access modifier. Object type or member can be accessed only inside declare type
        /// </summary>
        Private = DSharpTokenType.Private,
        /// <summary>
        /// Protected access modifier. Object type or member can be accessed from all inherited types
        /// </summary>
        Protected = DSharpTokenType.Protected,
        /// <summary>
        /// Internal access modifier. Object type or member can be accessed only in assembly that contains it
        /// </summary>
        Internal = DSharpTokenType.Internal
    }
}

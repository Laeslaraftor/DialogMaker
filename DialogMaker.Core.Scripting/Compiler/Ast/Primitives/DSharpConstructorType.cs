namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    /// <summary>
    /// Type of constructor
    /// </summary>
    public enum DSharpConstructorType
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        Default,
        /// <summary>
        /// Constructor that invoke other constructor in current type
        /// </summary>
        ThisInvocation,
        /// <summary>
        /// Constructor that invoke other constructor in base type
        /// </summary>
        BaseInvocation
    }
}

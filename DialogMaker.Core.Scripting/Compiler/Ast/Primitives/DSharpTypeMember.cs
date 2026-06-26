namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    /// <summary>
    /// Type of member
    /// </summary>
    public enum DSharpTypeMember
    {
        /// <summary>
        /// Constructor of type
        /// </summary>
        Constructor,
        /// <summary>
        /// Field/property
        /// </summary>
        Field,
        /// <summary>
        /// Method
        /// </summary>
        Method,
        /// <summary>
        /// Finalizer/destructor
        /// </summary>
        Finalizer,
        /// <summary>
        /// Indexer (this[int] { get; set; })
        /// </summary>
        Indexer,
    }
}

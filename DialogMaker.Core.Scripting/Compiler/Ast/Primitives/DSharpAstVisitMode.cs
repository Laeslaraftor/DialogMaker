namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    /// <summary>
    /// D# abstract syntax tree visit mode
    /// </summary>
    public enum DSharpAstVisitMode
    {
        /// <summary>
        /// Visit only current node
        /// </summary>
        Simple,
        /// <summary>
        /// Visit current node and it's children
        /// </summary>
        Children,
        /// <summary>
        /// Visit current node and recursive visit children
        /// </summary>
        Recursive
    }
}

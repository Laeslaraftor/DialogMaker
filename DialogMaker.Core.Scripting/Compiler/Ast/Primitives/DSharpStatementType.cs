namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    /// <summary>
    /// Statement type
    /// </summary>
    public enum DSharpStatementType
    {
        /// <summary>
        /// Declaration statement
        /// </summary>
        Declaration,
        /// <summary>
        /// Code statement
        /// </summary>
        Code,
        /// <summary>
        /// Declaration or code statement
        /// </summary>
        Any,
    }
}

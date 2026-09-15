namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    /// <summary>
    /// Mode of access to member
    /// </summary>
    public enum DSharpMemberAccessMode
    {
        /// <summary>
        /// Access to member through reference (.)
        /// </summary>
        Reference,
        /// <summary>
        /// Access to member through pointer (->)
        /// </summary>
        Pointer
    }
}

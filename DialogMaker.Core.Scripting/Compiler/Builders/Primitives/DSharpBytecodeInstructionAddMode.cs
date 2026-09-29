namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    /// <summary>
    /// Mode of instruction adding
    /// </summary>
    public enum DSharpBytecodeInstructionAddMode
    {
        /// <summary>
        /// Add instructions to end of list
        /// </summary>
        End,
        /// <summary>
        /// Add instructions to start of list
        /// </summary>
        Start,
        /// <summary>
        /// Insert instructions at specified index
        /// </summary>
        Index,
        /// <summary>
        /// Insert instructions at specified index and increase index by 1
        /// </summary>
        IndexWithCounting,
    }
}

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Mode of stack value
    /// </summary>
    public enum DSharpStackValueMode : byte
    {
        /// <summary>
        /// Default stack value
        /// </summary>
        Default,
        /// <summary>
        /// Stack value is number
        /// </summary>
        Number,
        /// <summary>
        /// Stack value is pointer to pointer (void**)
        /// </summary>
        PointerToPointer
    }
}

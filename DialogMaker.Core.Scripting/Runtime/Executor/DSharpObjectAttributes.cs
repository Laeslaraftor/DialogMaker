namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# object attributes
    /// </summary>
    [Flags]
    public enum DSharpObjectAttributes : short
    {
        /// <summary>
        /// Empty attribute
        /// </summary>
        None = 0,
        /// <summary>
        /// Object stores in buffer (for value types)
        /// </summary>
        StoredInBuffer = 1 << 1,
        /// <summary>
        /// Object stores in heap (for reference types)
        /// </summary>
        StoredInHeap = 1 << 2,
        /// <summary>
        /// Object is array (<c>string</c> also counts as array)
        /// </summary>
        Array = 1 << 3,
        /// <summary>
        /// Object is <c>string</c>
        /// </summary>
        String = 1 << 4,
        /// <summary>
        /// Object initialized
        /// </summary>
        Initialized = 1 << 5
    }
}

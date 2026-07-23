namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Storage that contains D# object
    /// </summary>
    public enum DSharpObjectPlacement
    {
        /// <summary>
        /// Object stores in some buffer (stack or field)
        /// </summary>
        Buffer = DSharpObjectAttributes.StoredInBuffer,
        /// <summary>
        /// Object stores in heap
        /// </summary>
        Heap = DSharpObjectAttributes.StoredInHeap
    }
}

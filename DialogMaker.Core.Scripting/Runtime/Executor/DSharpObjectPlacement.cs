namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Storage that contains D# object
    /// </summary>
    public enum DSharpObjectPlacement : byte
    {
        /// <summary>
        /// Object stores in some buffer (stack or field)
        /// </summary>
        Buffer,
        /// <summary>
        /// Object stores in heap
        /// </summary>
        Heap
    }
}

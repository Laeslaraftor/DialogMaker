using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Information about locking D# object
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DSharpObjectLockInfo
    {
        /// <summary>
        /// Is object now locked
        /// </summary>
        public bool Locked;
        /// <summary>
        /// Thread id that now using locked object
        /// </summary>
        public int OwnerThreadId;
        /// <summary>
        /// Count of locking in owner thread
        /// </summary>
        public int RecursiveCount;
    }
}

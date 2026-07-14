using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# code execution scope
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DSharpExecutionScope
    {
        /// <summary>
        /// Count of elements in stack at scope start
        /// </summary>
        public uint StackCount;
    }
}

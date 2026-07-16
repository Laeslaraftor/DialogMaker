using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Catch block information
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpCatchBlock
    {
        /// <summary>
        /// Index of catch block first instruction 
        /// </summary>
        public uint InstructionIndex;
        /// <summary>
        /// Exception type that accepting by this block.
        /// Empty field means that this catch block accept all exceptions
        /// </summary>
        public DSharpRuntimeTypeInfo* ExceptionType;
    }
}

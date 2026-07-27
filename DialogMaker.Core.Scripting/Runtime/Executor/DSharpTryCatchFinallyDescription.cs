using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Description of catch or finally block
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpTryCatchFinallyDescription
    {
        /// <summary>
        /// Identifier of try-catch-finally block that contains current catch/finally block
        /// </summary>
        public int TryCatchFinallyBlockId;
        /// <summary>
        /// Index of catch block first instruction 
        /// </summary>
        public uint InstructionIndex;
        /// <summary>
        /// Is this description of finally block
        /// </summary>
        public bool IsFinallyBlock;
        /// <summary>
        /// Exception type that accepting by this catch block.
        /// Empty field means that this catch block accept all exceptions
        /// </summary>
        public DSharpRuntimeTypeInfo* ExceptionType;
    }
}

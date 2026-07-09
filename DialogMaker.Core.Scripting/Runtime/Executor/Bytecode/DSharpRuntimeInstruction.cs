using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode
{
    /// <summary>
    /// Information about runtime instruction
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct DSharpRuntimeInstruction
    {
        /// <summary>
        /// D# operation
        /// </summary>
        public DSharpBytecodeOperation Operation => _operation;

        private readonly DSharpBytecodeOperation _operation;
        private readonly int _argumentsLength;
        private readonly nint _arguments;

        #region Controls

        /// <summary>
        /// Get instruction arguments stream.
        /// It allows to read arguments
        /// </summary>
        /// <returns>Stream of unmanaged memory that contains arguments</returns>
        public UnmanagedStream GetArgumentsStream() => new(_arguments, _argumentsLength);

        #endregion
    }
}

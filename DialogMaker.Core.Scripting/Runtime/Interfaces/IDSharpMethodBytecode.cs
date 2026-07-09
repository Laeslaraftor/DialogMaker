using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime.Executor;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Interface of method bytecode
    /// </summary>
    public interface IDSharpMethodBytecode
    {
        /// <summary>
        /// Size of bytecode in bytes
        /// </summary>
        public int Size { get; }
        /// <summary>
        /// Total amount of instructions
        /// </summary>
        public int InstructionsCount { get; }

        /// <summary>
        /// Copy bytecode of current method to specified bytecode builder
        /// </summary>
        /// <param name="builder">Bytecode builder for copying bytecode</param>
        public void CopyTo(DSharpBytecodeBuilder builder);
        /// <summary>
        /// Copy bytecode of current method to specified unmanaged byte array
        /// </summary>
        /// <param name="byteArray">Unmanaged byte array for copying bytecode</param>
        public void CopyTo(UnmanagedArray<byte> byteArray);
    }
}

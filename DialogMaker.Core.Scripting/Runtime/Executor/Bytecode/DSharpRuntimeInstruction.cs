using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode
{
    /// <summary>
    /// Information about runtime instruction
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpRuntimeInstruction
    {
        /// <summary>
        /// D# operation
        /// </summary>
        public DSharpBytecodeOperation Operation;
        /// <summary>
        /// Array of pointers to operation arguments
        /// </summary>
        public UnmanagedArray<nint> Arguments;
        /// <summary>
        /// Pointer to executor method that implements <see cref="DSharpInstructionExecutor.Execute(DSharpRuntimeInstruction, DSharpExecutionContext)"/>
        /// </summary>
        public delegate*<DSharpRuntimeInstruction, DSharpExecutionContext, void> Executor;

        #region Controls

        /// <summary>
        /// Execute instruction
        /// </summary>
        /// <param name="context">Execution context</param>
        /// <exception cref="InvalidOperationException">Executor not specified</exception>
        public readonly void Execute(DSharpExecutionContext context)
        {
            if (Executor == null)
            {
                throw new InvalidOperationException("Executor not specified");
            }

            Executor(this, context);
        }

        #endregion

        #region Static

        /// <summary>
        /// Get arguments count of current instruction
        /// </summary>
        /// <param name="typesProvider">Types provider for getting runtime type information</param>
        /// <param name="stream">Bytecode stream</param>
        /// <returns>Arguments count of current instruction</returns>
        public static int GetArgumentsCount(DSharpRuntimeTypesProvider typesProvider, ref UnmanagedStream stream)
        {
            var operation = stream.Read<DSharpBytecodeOperation>();
            var argsCountAttribute = operation.GetEnumAttribute<ArgsCountAttribute>();

            if (argsCountAttribute == null || argsCountAttribute.Count == 0)
            {
                return sizeof(DSharpBytecodeOperation);
            }

            return -1;
        }

        #endregion
    }
}

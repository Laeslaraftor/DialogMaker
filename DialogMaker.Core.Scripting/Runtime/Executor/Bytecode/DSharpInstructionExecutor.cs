using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode
{
    /// <summary>
    /// Base class of D# instruction executor
    /// </summary>
    public abstract class DSharpInstructionExecutor
    {
        #region Controls

        /// <summary>
        /// Get pointer to executor method
        /// </summary>
        /// <returns>Pointer to executor method</returns>
        public unsafe abstract delegate*<DSharpRuntimeInstruction, DSharpExecutionContext, void> GetExecutorPointer();

        /// <summary>
        /// Execute instruction
        /// </summary>
        /// <param name="instruction">Executing instruction information</param>
        /// <param name="context">Execution context</param>
        public abstract void Execute(DSharpRuntimeInstruction instruction, DSharpExecutionContext context);
        /// <summary>
        /// Get count of arguments
        /// </summary>
        /// <param name="typesProvider">Runtime types provider for finding parameter type by metadata token</param>
        /// <param name="stream">Unmanaged stream for reading parameter information</param>
        /// <returns>Count of arguments</returns>
        public abstract int GetArgumentsCount(DSharpRuntimeTypesProvider typesProvider, UnmanagedStream stream);

        #endregion

        #region Static

        /// <summary>
        /// Try get executor implementation for D# operation
        /// </summary>
        /// <param name="operation">D# operation</param>
        /// <param name="result">Instruction executor that was found for specified operation</param>
        /// <returns>Is executor found</returns>
        public static bool TryGetExecutor(DSharpBytecodeOperation operation, [NotNullWhen(true)] out DSharpInstructionExecutor? result)
        {
            result = null;
            return false;
        }

        #endregion
    }
}

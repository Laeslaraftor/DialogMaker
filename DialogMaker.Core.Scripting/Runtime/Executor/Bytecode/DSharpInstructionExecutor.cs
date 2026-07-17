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
        public unsafe abstract delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer();

        /// <summary>
        /// Execute instruction
        /// </summary>
        /// <param name="instruction">Executing instruction information</param>
        /// <param name="context">Execution context</param>
        /// <returns>Is successfully executed</returns>
        public abstract DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context);
        /// <summary>
        /// Get count of arguments
        /// </summary>
        /// <param name="typesProvider">Runtime types provider for finding parameter type by metadata token</param>
        /// <param name="stream">Unmanaged stream for reading parameter information</param>
        /// <returns>Count of arguments</returns>
        public abstract int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream);
        /// <summary>
        /// Read arguments from stream and write it to arguments array
        /// </summary>
        /// <param name="typesProvider">Runtime types provider for finding parameter type by metadata token</param>
        /// <param name="stream">Unmanaged stream for reading parameter information</param>
        /// <param name="arguments">Array for writing arguments</param>
        public abstract void ReadArguments(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream, UnmanagedArray<nint> arguments);

        /// <summary>
        /// Check provided arguments count
        /// </summary>
        /// <param name="instruction">Instruction for checking provided arguments</param>
        /// <param name="context">Current execution context</param>
        /// <param name="requiredArgumentsCount">Requires amount of arguments</param>
        /// <returns>Is arguments count matched</returns>
        protected bool CheckArguments(DSharpRuntimeInstruction instruction, DSharpExecutionContext context, int requiredArgumentsCount, [NotNullWhen(true)] out DSharpMethodExecutionCallback errorCallback)
        {
            if (instruction.Arguments.Length != requiredArgumentsCount)
            {
                errorCallback = context.ThrowExecutionException($"{instruction.Operation} instruction requires {requiredArgumentsCount} arguments, got: {instruction.Arguments.Length}");
                return true;
            }

            errorCallback = default;
            return false;
        }
        /// <summary>
        /// Check current stack values count
        /// </summary>
        /// <param name="instruction">Current instruction</param>
        /// <param name="context">Current execution context</param>
        /// <param name="requiredValuesCount">Requires amount of values is stack</param>
        /// <returns>Is stack values enough</returns>
        protected bool CheckStackValues(DSharpRuntimeInstruction instruction, DSharpExecutionContext context, int requiredValuesCount, [NotNullWhen(true)] out DSharpMethodExecutionCallback errorCallback)
        {
            if (context.Stack.Count < requiredValuesCount)
            {
                errorCallback = context.ThrowExecutionException($"{instruction.Operation} instruction requires {requiredValuesCount} values in stack, now: {context.Stack.Count}");
                return true;
            }

            errorCallback = default;
            return false;
        }

        #endregion

        #region Static

        private static Dictionary<DSharpBytecodeOperation, DSharpInstructionExecutor>? _executors = [];

        /// <summary>
        /// Try get executor implementation for D# operation
        /// </summary>
        /// <param name="operation">D# operation</param>
        /// <param name="result">Instruction executor that was found for specified operation</param>
        /// <returns>Is executor found</returns>
        public static bool TryGetExecutor(DSharpBytecodeOperation operation, [NotNullWhen(true)] out DSharpInstructionExecutor? result)
        {
            var executors = GetExecutors();
            return executors.TryGetValue(operation, out result);
        }

        private static Dictionary<DSharpBytecodeOperation, DSharpInstructionExecutor> GetExecutors()
        {
            if (_executors != null)
            {
                return _executors;
            }

            _executors = [];

            foreach (var value in Enum.GetValues(typeof(DSharpBytecodeOperation)))
            {
                var executorAttribute = value.GetEnumAttribute<ExecutorAttribute>();

                if (executorAttribute != null)
                {
                    var instance = executorAttribute.GetInstance();
                    _executors.Add((DSharpBytecodeOperation)value, instance);
                }
            }

            return _executors;
        }

        #endregion
    }
}

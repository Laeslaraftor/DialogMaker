using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode
{
    /// <summary>
    /// Base class of D# instruction executor
    /// </summary>
    public abstract unsafe class DSharpInstructionExecutor
    {
        #region Controls

        /// <summary>
        /// Get pointer to executor method
        /// </summary>
        /// <returns>Pointer to executor method</returns>
        public abstract delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer();

        /// <summary>
        /// Execute instruction
        /// </summary>
        /// <param name="instruction">Executing instruction information</param>
        /// <param name="context">Execution context</param>
        /// <returns>Is successfully executed</returns>
        public abstract void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context);
        /// <summary>
        /// Get count of arguments
        /// </summary>
        /// <param name="typesProvider">Runtime types provider for finding parameter type by metadata token</param>
        /// <param name="stream">Unmanaged stream for reading parameter information</param>
        /// <returns>Count of arguments</returns>
        public abstract int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream);
        /// <summary>
        /// Read arguments from stream and write it to arguments array
        /// </summary>
        /// <param name="typesProvider">Runtime types provider for finding parameter type by metadata token</param>
        /// <param name="stream">Unmanaged stream for reading parameter information</param>
        /// <param name="arguments">Array for writing arguments</param>
        public abstract void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments);

#if DEBUG
        /// <summary>
        /// Check provided arguments count
        /// </summary>
        /// <param name="instruction">Instruction for checking provided arguments</param>
        /// <param name="context">Current execution context</param>
        /// <param name="requiredArgumentsCount">Requires amount of arguments</param>
        /// <returns>Is arguments count matched</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool CheckArguments(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, int requiredArgumentsCount)
        {
            if (instruction->Arguments.Length != requiredArgumentsCount)
            {
                *context.Callback = context.ThrowExecutionException($"{instruction->Operation} instruction requires {requiredArgumentsCount} arguments, got: {instruction->Arguments.Length}");
                return true;
            }

            return false;
        }
        /// <summary>
        /// Check current stack values count
        /// </summary>
        /// <param name="instruction">Current instruction</param>
        /// <param name="context">Current execution context</param>
        /// <param name="requiredValuesCount">Requires amount of values is stack</param>
        /// <returns>Is stack values enough</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool CheckStackValues(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, int requiredValuesCount)
        {
            if (context.Stack.Count < requiredValuesCount)
            {
                *context.Callback = context.ThrowExecutionException($"{instruction->Operation} instruction requires {requiredValuesCount} values in stack, now: {context.Stack.Count}");
                return true;
            }

            return false;
        }
#endif
        /// <summary>
        /// Get object instance from stack with specified offset
        /// </summary>
        /// <param name="context">Current execution context</param>
        /// <param name="offset">Stack peek offset</param>
        /// <returns>Pointer to object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static DSharpObject* GetInstance(ref DSharpExecutionContext context, uint offset)
        {
            return GetInstance(ref context, offset, false);
        }
        /// <summary>
        /// Get object instance from stack with specified offset
        /// </summary>
        /// <param name="context">Current execution context</param>
        /// <param name="offset">Stack peek offset</param>
        /// <param name="error">Error that occurred while finding object instance</param>
        /// <returns>Pointer to object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static DSharpObject* GetInstance(ref DSharpExecutionContext context, uint offset, bool throwOnError)
        {
            var instanceFrame = context.Stack.PeekOnlyValues(offset);

            if (instanceFrame.ValueType == DSharpStackValueType.Null)
            {
                if (throwOnError)
                {
                    *context.Callback = context.ThrowExecutionException("Null object reference");
                }

                return null;
            }

            DSharpObject* instance;

            if (instanceFrame.ValueType == DSharpStackValueType.Reference)
            {
                instance = (DSharpObject*)instanceFrame.ReadReference();
            }
            else if (instanceFrame.ValueType == DSharpStackValueType.Structure)
            {
                instance = (DSharpObject*)instanceFrame.StackPointer;
            }
            else
            {
                if (throwOnError)
                {
                    *context.Callback = context.ThrowExecutionException($"Invalid instance value: {instanceFrame.ValueType}");
                }

                return null;
            }

            return instance;
        }

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
            if (DSharpOperationExecutorInfos.Operations.TryGetValue(operation, out var info))
            {
                result = info.ExecutorInstance;
                return true;
            }

            result = null;
            return false;
        }

        #endregion
    }
}

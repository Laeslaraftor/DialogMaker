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
        public unsafe abstract int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream);
        /// <summary>
        /// Read arguments from stream and write it to arguments array
        /// </summary>
        /// <param name="typesProvider">Runtime types provider for finding parameter type by metadata token</param>
        /// <param name="stream">Unmanaged stream for reading parameter information</param>
        /// <param name="arguments">Array for writing arguments</param>
        public unsafe abstract void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments);

        /// <summary>
        /// Check provided arguments count
        /// </summary>
        /// <param name="instruction">Instruction for checking provided arguments</param>
        /// <param name="context">Current execution context</param>
        /// <param name="requiredArgumentsCount">Requires amount of arguments</param>
        /// <returns>Is arguments count matched</returns>
        protected static bool CheckArguments(DSharpRuntimeInstruction instruction, DSharpExecutionContext context, int requiredArgumentsCount, [NotNullWhen(true)] out DSharpMethodExecutionCallback errorCallback)
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
        protected static bool CheckStackValues(DSharpRuntimeInstruction instruction, DSharpExecutionContext context, int requiredValuesCount, [NotNullWhen(true)] out DSharpMethodExecutionCallback errorCallback)
        {
            if (context.Stack.Count < requiredValuesCount)
            {
                errorCallback = context.ThrowExecutionException($"{instruction.Operation} instruction requires {requiredValuesCount} values in stack, now: {context.Stack.Count}");
                return true;
            }

            errorCallback = default;
            return false;
        }
        /// <summary>
        /// Get object instance from stack with specified offset
        /// </summary>
        /// <param name="context">Current execution context</param>
        /// <param name="offset">Stack peek offset</param>
        /// <param name="error">Error that occurred while finding object instance</param>
        /// <returns>Pointer to object instance</returns>
        protected unsafe static DSharpObject* GetInstance(DSharpExecutionContext context, uint offset, [NotNullWhen(true)] out DSharpMethodExecutionCallback error)
        {
            var instanceFrame = context.Stack.Peek(offset);

            if (instanceFrame.ValueType == DSharpStackValueType.Null)
            {
                error = context.ThrowExecutionException("Null object reference");
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
                error = context.ThrowExecutionException($"Invalid instance value: {instanceFrame.ValueType}");
                return null;
            }

            error = DSharpMethodExecutionCallback.Complete();

            return instance;
        }

        /// <summary>
        /// Try to get object instance from stack.
        /// It can be reference to object or structure
        /// </summary>
        /// <param name="context">Current execution context</param>
        /// <param name="handler">Object handler</param>
        /// <returns>Result of operation</returns>
        protected unsafe DSharpMethodExecutionCallback TryGetInstanceFromStack(ref DSharpExecutionContext context, HandleObject handler)
        {
            var stackValue = context.Stack.Peek();
            DSharpObject* instance;
            int instanceBufferSize;

            if (stackValue.ValueType == DSharpStackValueType.Reference)
            {
                instance = (DSharpObject*)stackValue.ReadReference();
                goto HandleObject;
            }
            else if (DSharpObjectsContainer.TryGetSizeForStructureFromStack(context.Stack, out var size))
            {
                instanceBufferSize = size;
                goto CreateStructure;
            }
            else
            {
                return context.ThrowExecutionException("No instance provided");
            }

        CreateStructure:
            byte* structureBuffer = stackalloc byte[instanceBufferSize];
            UnmanagedArray<byte> buffer = new(structureBuffer, instanceBufferSize);
            instance = DSharpObjectsContainer.CreateStructureFromStack(context.Stack, buffer);
            goto HandleObject;

        HandleObject:
            return handler(ref context, instance);
        }

        #endregion

        #region Delegates

        /// <summary>
        /// Object handler
        /// </summary>
        /// <param name="context">Current execution context</param>
        /// <param name="obj">Pointer to object instance</param>
        protected unsafe delegate DSharpMethodExecutionCallback HandleObject(ref DSharpExecutionContext context, DSharpObject* obj);

        #endregion

        #region Static

        private static Dictionary<DSharpBytecodeOperation, DSharpInstructionExecutor>? _executors;

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

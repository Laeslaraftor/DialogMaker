using DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions;
using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# execution context
    /// </summary>
    public readonly unsafe struct DSharpExecutionContext(DSharpObjectsContainer objectsContainer, DSharpThread thread, DSharpRuntimeMethodInfo* currentMethod, DSharpMethodExecutor* executor)
    {
        /// <summary>
        /// Runtime types provider
        /// </summary>
        public DSharpRuntimeInformationProvider TypesProvider { get; } = thread.Executor.RuntimeTypesProvider;
        /// <summary>
        /// Current executing thread
        /// </summary>
        public DSharpStack Stack { get; } = thread.Stack;
        /// <summary>
        /// Current instruction index
        /// </summary>
        public uint InstructionIndex
        {
            get => _executor->InstructionIndex;
            set => _executor->InstructionIndex = value;
        }
        /// <summary>
        /// Container of all created object
        /// </summary>
        public DSharpObjectsContainer ObjectsContainer { get; } = objectsContainer;
        /// <summary>
        /// Current object instance. 
        /// It's property null when current member is static
        /// </summary>
        public DSharpObject* ObjectInstance => _executor->ObjectInstance;
        /// <summary>
        /// Runtime information about current executing method
        /// </summary>
        public DSharpRuntimeMethodInfo* CurrentMethod { get; } = currentMethod;
        /// <summary>
        /// Current method local variables
        /// </summary>
        public UnmanagedArray<DSharpExecutionLocalVariable>* LocalVariables => &_executor->LocalVariables;
        /// <summary>
        /// <inheritdoc cref="DSharpMethodExecutor.CurrentTryCatchFinallyId"/>
        /// </summary>
        public int CurrentTryCatchFinallyId => _executor->CurrentTryCatchFinallyId;
        /// <summary>
        /// <inheritdoc cref="DSharpMethodExecutor.NextReturnInstructions"/>
        /// </summary>
        public UnmanagedList<uint>* NextReturnInstructions => &_executor->NextReturnInstructions;
        /// <summary>
        /// <inheritdoc cref="DSharpMethodExecutor.NowClosingTryCatchFinallyBlock"/>
        /// </summary>
        public bool NowClosingTryCatchFinallyBlock => _executor->NowClosingTryCatchFinallyBlock;

        private readonly DSharpMethodExecutor* _executor = executor;

        #region Controls

        /// <summary>
        /// Start new scope
        /// </summary>
        /// <returns>Started scope</returns>
        /// <exception cref="InvalidOperationException">Unable to get next scope because all allocated scopes already used</exception>
        public DSharpStack.Scope StartScope()
        {
            var scopes = &_executor->LocalScopes;

            if (scopes->Count + 1 > scopes->Capacity)
            {
                throw new InvalidOperationException("Unable to get next scope because all allocated scopes already used");
            }

            var scope = Stack.StartScope();
            scopes->Add(scope);

            return scope;
        }
        /// <summary>
        /// Close current scope and remove all it values from stack
        /// </summary>
        public void CloseCurrentScope()
        {
            var scopes = &_executor->LocalScopes;

            if (scopes->Count == 0)
            {
                return;
            }

            int lastIndex = scopes->Count - 1;
            var scope = _executor->LocalScopes[lastIndex];
            Stack.CloseScope(scope, 0);
            scopes->RemoveAt(lastIndex);
        }

        /// <summary>
        /// Start new try-catch-finally block
        /// </summary>
        /// <returns>Identifier of started try-catch-finally block</returns>
        public int StartTryCatchFinally()
        {
            return _executor->CurrentTryCatchFinallyId++;
        }
        /// <summary>
        /// End current try-catch-finally block
        /// </summary>
        /// <returns>Is try-catch-finally block ended</returns>
        public bool EndTryCatchFinally() => EndTryCatchFinally(_executor);

        /// <summary>
        /// Add finally block start instruction index for current try-catch-finally block
        /// </summary>
        /// <param name="instructionIndex">Index of instruction that represents start of finally block</param>
        /// <returns>Is finally block added</returns>
        public bool AddFinallyBlock(int instructionIndex)
        {
            int currentId = _executor->CurrentTryCatchFinallyId;

            for (int i = _executor->TryCatchFinallyDescriptions.Count - 1; i >= 0; i--)
            {
                var description = _executor->TryCatchFinallyDescriptions[i];

                if (description.TryCatchFinallyBlockId == currentId &&
                    description.IsFinallyBlock)
                {
                    return false;
                }
            }

            _executor->TryCatchFinallyDescriptions.Add(new()
            {
                TryCatchFinallyBlockId = currentId,
                InstructionIndex = (uint)instructionIndex,
                IsFinallyBlock = true
            });

            return true;
        }
        /// <summary>
        /// Add catch block for current try-catch-finally block
        /// </summary>
        /// <param name="exceptionType">Type of exception that specified catch block should handling</param>
        /// <param name="instructionIndex">Index of instruction that represents start of catch block</param>
        /// <returns>Is catch block added</returns>
        public bool AddCatchBlock(DSharpRuntimeTypeInfo* exceptionType, int instructionIndex)
        {
            int currentId = _executor->CurrentTryCatchFinallyId;

            for (int i = _executor->TryCatchFinallyDescriptions.Count - 1; i >= 0; i--)
            {
                var description = _executor->TryCatchFinallyDescriptions[i];

                if (description.TryCatchFinallyBlockId == currentId &&
                    !description.IsFinallyBlock &&
                    description.ExceptionType == exceptionType)
                {
                    return false;
                }
            }

            _executor->TryCatchFinallyDescriptions.Add(new()
            {
                TryCatchFinallyBlockId = currentId,
                InstructionIndex = (uint)instructionIndex,
                IsFinallyBlock = false,
                ExceptionType = exceptionType
            });

            return true;
        }
        /// <summary>
        /// Try to get current try-catch-finally block finally start instruction index
        /// </summary>
        /// <param name="result">Instruction index of start finally block</param>
        /// <returns>Is instruction index successfully found</returns>
        public bool TryGetCurrentFinallyBlockInstructionIndex(out uint result)
        {
            int currentId = _executor->CurrentTryCatchFinallyId;

            for (int i = _executor->TryCatchFinallyDescriptions.Count - 1; i >= 0; i--)
            {
                var description = _executor->TryCatchFinallyDescriptions[i];

                if (description.TryCatchFinallyBlockId == currentId &&
                    description.IsFinallyBlock)
                {
                    result = description.InstructionIndex;
                    return true;
                }
            }

            result = 0;
            return false;
        }

        /// <summary>
        /// Throw execution engine exception.
        /// This exception will be thrown in virtual machine, not in C#
        /// </summary>
        /// <param name="message">Exception message</param>
        public DSharpMethodExecutionCallback ThrowExecutionException(string message)
        {
            var throwMethod = TypesProvider.RuntimeHelperType.ThrowExecutionEngineExceptionMethod;
            var runtimeThrowMethod = TypesProvider.GetMethod(throwMethod.MetadataToken);

            var messageInstance = ObjectsContainer.CreateString(message);
            Stack.PushReference(messageInstance);
            var args = DSharpCallInstructionExecutor.CreateArguments(this, runtimeThrowMethod);

            return DSharpMethodExecutionCallback.Call(null, runtimeThrowMethod, args);
        }
        /// <summary>
        /// Throw execution engine exception.
        /// This exception will be thrown in virtual machine, not in C#
        /// </summary>
        /// <param name="exception">Exception for throwing</param>
        public DSharpMethodExecutionCallback ThrowExecutionException(Exception exception)
        {
            return ThrowExecutionException(exception.ToString());
        }

        #endregion

        #region Static

        /// <summary>
        /// End current try-catch-finally block
        /// </summary>
        /// <returns>Is try-catch-finally block ended</returns>
        public static bool EndTryCatchFinally(DSharpMethodExecutor* executor)
        {
            int currentId = executor->CurrentTryCatchFinallyId;

            if (0 >= currentId)
            {
                return false;
            }

            while (true)
            {
                bool isDescriptionRemoved = false;

                for (int i = executor->TryCatchFinallyDescriptions.Count - 1; i >= 0; i--)
                {
                    var description = executor->TryCatchFinallyDescriptions[i];

                    if (description.TryCatchFinallyBlockId == currentId)
                    {
                        executor->TryCatchFinallyDescriptions.RemoveAt(i);
                        isDescriptionRemoved = true;
                        break;
                    }
                }

                if (!isDescriptionRemoved)
                {
                    break;
                }
            }

            executor->CurrentTryCatchFinallyId--;

            return true;
        }

        #endregion
    }
}

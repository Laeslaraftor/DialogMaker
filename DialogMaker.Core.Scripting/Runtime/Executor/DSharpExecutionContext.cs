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

            if (scopes->Count + 1 >= scopes->Capacity)
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
        /// Throw execution engine exception.
        /// This exception will be thrown in virtual machine, not in C#
        /// </summary>
        /// <param name="message">Exception message</param>
        [Obsolete("todo: remove InvalidOperationException and add throwing exception directly in virtual machine")]
        public DSharpMethodExecutionCallback ThrowExecutionException(string message)
        {
            throw new InvalidOperationException(message);
            return DSharpMethodExecutionCallback.Throw(null);
        }
        /// <summary>
        /// Throw execution engine exception.
        /// This exception will be thrown in virtual machine, not in C#
        /// </summary>
        /// <param name="exception">Exception for throwing</param>
        [Obsolete("todo: remove InvalidOperationException and add throwing exception directly in virtual machine")]
        public DSharpMethodExecutionCallback ThrowExecutionException(Exception exception)
        {
            throw exception;
            return ThrowExecutionException(exception.ToString());
        }

        #endregion
    }
}

using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# execution context
    /// </summary>
    public unsafe struct DSharpExecutionContext(DSharpObjectsContainer objectsContainer, DSharpThread thread, DSharpObject* instance, DSharpRuntimeMethodInfo* currentMethod)
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
            get => *_instructionIndex;
            set => *_instructionIndex = value;
        }
        /// <summary>
        /// Container of all created object
        /// </summary>
        public DSharpObjectsContainer ObjectsContainer { get; } = objectsContainer;
        /// <summary>
        /// Current execution scopes
        /// </summary>
        public UnmanagedArray<DSharpExecutionScope> Scopes { get; }
        /// <summary>
        /// Current object instance. 
        /// It's property null when current member is static
        /// </summary>
        public DSharpObject* ObjectInstance { get; } = instance;
        /// <summary>
        /// Runtime information about current executing method
        /// </summary>
        public DSharpRuntimeMethodInfo* CurrentMethod { get; } = currentMethod;
        /// <summary>
        /// Current execution scope
        /// </summary>
        public readonly DSharpExecutionScope CurrentScope => Scopes[_currentScopeIndex];

        private int _currentScopeIndex;
        private uint* _instructionIndex;

        #region Controls

        /// <summary>
        /// Start new scope
        /// </summary>
        /// <returns>Started scope</returns>
        /// <exception cref="InvalidOperationException">Unable to get next scope because all allocated scopes already used</exception>
        public DSharpExecutionScope StartScope()
        {
            if (_currentScopeIndex + 1 >= Scopes.Length)
            {
                throw new InvalidOperationException("Unable to get next scope because all allocated scopes already used");
            }

            _currentScopeIndex++;
            DSharpExecutionScope scope = new()
            {
                StackCount = Stack.Count
            };
            var scopes = Scopes;
            scopes[_currentScopeIndex] = scope;

            return scope;
        }
        /// <summary>
        /// Close current scope and remove all it values from stack
        /// </summary>
        public void CloseCurrentScope()
        {
            if (_currentScopeIndex == 0)
            {
                return;
            }

            var currentScope = CurrentScope;

            while (Stack.Count > currentScope.StackCount)
            {
                Stack.Pop();
            }

            _currentScopeIndex--;
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

        #endregion
    }
}

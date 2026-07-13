using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# execution context
    /// </summary>
    public unsafe struct DSharpExecutionContext(DSharpRuntimeTypesProvider typesProvider, DSharpStack stack, CancellationToken cancellationToken, DSharpObject* instance)
    {
        /// <summary>
        /// Runtime types provider
        /// </summary>
        public DSharpRuntimeTypesProvider TypesProvider { get; } = typesProvider;
        /// <summary>
        /// Current executing thread
        /// </summary>
        public DSharpStack Stack { get; } = stack;
        /// <summary>
        /// Current instruction index
        /// </summary>
        public int InstructionIndex { get; set; }
        /// <summary>
        /// Token for cancelling execution
        /// </summary>
        public CancellationToken CancellationToken { get; } = cancellationToken;
        /// <summary>
        /// Current object instance. 
        /// It's property null when current member is static
        /// </summary>
        public DSharpObject* ObjectInstance { get; } = instance;
    }
}

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# execution context
    /// </summary>
    public unsafe readonly struct DSharpExecutionContext
    {
        /// <summary>
        /// Current executing thread
        /// </summary>
        public DSharpThread Thread { get; }
        /// <summary>
        /// Token for cancelling execution
        /// </summary>
        public CancellationToken CancellationToken { get; }
        /// <summary>
        /// Current object instance. 
        /// It's property null when current member is static
        /// </summary>
        public DSharpObject* ObjectInstance { get; }
    }
}

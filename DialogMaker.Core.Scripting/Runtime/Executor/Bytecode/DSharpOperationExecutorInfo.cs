namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode
{
    /// <summary>
    /// Information about bytecode operation and it's executor
    /// </summary>
    internal unsafe readonly struct DSharpOperationExecutorInfo(DSharpBytecodeOperation operation, DSharpInstructionExecutor executorInstance)
    {
        /// <summary>
        /// Operation for which this information is purposed
        /// </summary>
        public readonly DSharpBytecodeOperation Operation = operation;
        /// <summary>
        /// Current operation executor instance
        /// </summary>
        public readonly DSharpInstructionExecutor ExecutorInstance = executorInstance;
        /// <summary>
        /// Pointer to method for execute operation
        /// </summary>
        public readonly delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> ExecutorMethod = executorInstance.GetExecutorPointer();
    }
}

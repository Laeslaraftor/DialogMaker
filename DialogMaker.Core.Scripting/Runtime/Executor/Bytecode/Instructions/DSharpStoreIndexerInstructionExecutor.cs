namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.StoreIndexer"/> operation
    /// </summary>
    public class DSharpStoreIndexerInstructionExecutor : DSharpMetadataTokenInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken)
        {
            return DSharpStorePropertyInstructionExecutor.Store(instruction, ref context, metadataToken, true, false);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.StoreIndexer"/> operation executor
        /// </summary>
        public static readonly DSharpStoreIndexerInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

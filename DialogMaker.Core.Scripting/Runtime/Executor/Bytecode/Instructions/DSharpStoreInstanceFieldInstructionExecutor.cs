using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.StoreInstanceField"/> operation
    /// </summary>
    public unsafe class DSharpStoreInstanceFieldInstructionExecutor : DSharpFieldInstructionExecutor
    {
        #region Controls

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, DSharpRuntimeFieldInfo* runtimeInfo)
        {
            DSharpStoreFieldInstructionExecutor.Store(instruction, ref context, runtimeInfo, true);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.StoreInstanceField"/> operation executor
        /// </summary>
        public static readonly DSharpStoreInstanceFieldInstructionExecutor Instance = new();

        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

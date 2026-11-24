using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.StoreProperty"/> operation
    /// </summary>
    public unsafe class DSharpStorePropertyInstructionExecutor : DSharpPropertyInstructionExecutor
    {
        #region Controls

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override DSharpRuntimeMethodInfo* GetAccessor(DSharpRuntimePropertyInfo* property)
        {
            return property->Setter;
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.StoreProperty"/> operation executor
        /// </summary>
        public static readonly DSharpStorePropertyInstructionExecutor Instance = new();

        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

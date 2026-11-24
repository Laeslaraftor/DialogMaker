using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.LoadTypeInformation"/> operation
    /// </summary>
    public unsafe class DSharpLoadTypeInformationInstructionExecutor : DSharpTypeInstructionExecutor
    {
        #region Controls


        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, DSharpRuntimeTypeInfo* runtimeInfo)
        {
            context.Stack.Push(runtimeInfo);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.LoadTypeInformation"/> operation executor
        /// </summary>
        public static readonly DSharpLoadTypeInformationInstructionExecutor Instance = new();

        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

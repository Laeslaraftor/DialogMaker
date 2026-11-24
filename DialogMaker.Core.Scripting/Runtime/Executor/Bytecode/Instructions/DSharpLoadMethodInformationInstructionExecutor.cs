using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.LoadMethodInformation"/> operation
    /// </summary>
    public unsafe class DSharpLoadMethodInformationInstructionExecutor : DSharpMetadataTokenInstructionExecutor<DSharpRuntimeMethodInfo>
    {
        #region Controls


        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, DSharpRuntimeMethodInfo* runtimeInfo)
        {
            context.Stack.Push(runtimeInfo);
        }
        protected override DSharpRuntimeMethodInfo* GetRuntimeInformation(DSharpRuntimeInformationProvider typesProvider, DSharpMetadataToken metadataToken)
        {
            return typesProvider.GetMethod(metadataToken);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.LoadMethodInformation"/> operation executor
        /// </summary>
        public static readonly DSharpLoadMethodInformationInstructionExecutor Instance = new();

        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

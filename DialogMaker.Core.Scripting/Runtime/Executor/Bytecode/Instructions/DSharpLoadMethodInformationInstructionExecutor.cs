using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.LoadMethodInformation"/> operation
    /// </summary>
    public unsafe class DSharpLoadMethodInformationInstructionExecutor : DSharpMetadataTokenInstructionExecutor<DSharpRuntimeMethodInfo>
    {
        #region Controls


        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimeMethodInfo* runtimeInfo)
        {
            context.Stack.Push(runtimeInfo);
            return DSharpMethodExecutionCallback.Complete();
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

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

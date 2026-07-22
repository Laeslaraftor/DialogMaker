namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.LoadBaseInstanceProperty"/> operation
    /// </summary>
    public class DSharpLoadBaseInstancePropertyInstructionExecutor : DSharpMetadataTokenInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken)
        {
            return DSharpLoadPropertyInstructionExecutor.Load(instruction, ref context, metadataToken, true, true);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.LoadBaseInstanceProperty"/> operation executor
        /// </summary>
        public static readonly DSharpLoadBaseInstancePropertyInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

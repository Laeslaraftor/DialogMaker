namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.GenericCall"/> operation
    /// </summary>
    public class DSharpGenericCallInstructionExecutor : DSharpGenericCallInstructionExecutorBase
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken methodToken, UnmanagedArray<Pointer<DSharpMetadataToken>> genericParameters)
        {
            return DSharpCallInstructionExecutor.Call(instruction, ref context, methodToken, false, false, genericParameters);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.GenericCall"/> operation executor
        /// </summary>
        public static readonly DSharpGenericCallInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.GreaterOrEqual"/> operation
    /// </summary>
    public unsafe class DSharpGreaterOrEqualInstructionExecutor : DSharpMathInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override DSharpNumber PerformMathOperation(in DSharpNumber left, in DSharpNumber right, ref DSharpExecutionContext context, out DSharpMethodExecutionCallback? error)
        {
            error = null;
            return left >= right;
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.GreaterOrEqual"/> operation executor
        /// </summary>
        public static readonly DSharpGreaterOrEqualInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

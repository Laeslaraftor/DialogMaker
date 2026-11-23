namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Divide"/> operation
    /// </summary>
    public unsafe class DSharpDivideInstructionExecutor : DSharpMathInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        protected override DSharpNumber PerformMathOperation(in DSharpNumber left, in DSharpNumber right, ref DSharpExecutionContext context, out DSharpMethodExecutionCallback? error)
        {
            try
            {
                error = null;
                return left / right;
            }
            catch (DivideByZeroException)
            {
                error = context.ThrowDivideByZeroException();
                return 0;
            }
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Divide"/> operation executor
        /// </summary>
        public static readonly DSharpDivideInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

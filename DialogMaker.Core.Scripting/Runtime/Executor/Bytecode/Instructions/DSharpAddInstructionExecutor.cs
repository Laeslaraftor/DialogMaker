namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Add"/> operation
    /// </summary>
    public class DSharpAddInstructionExecutor : DSharpMathInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override decimal PerformMathOperation(decimal left, decimal right)
        {
            return left + right;
        }
        protected override bool PerformMathOperation(bool left, bool right)
        {
            return false;
        }
        protected override unsafe bool CanPerform(DSharpStack.FrameInfo left, DSharpStack.FrameInfo right, DSharpExecutionContext context)
        {
            return left.ObjectType != context.TypesProvider.Boolean && left.ObjectType != context.TypesProvider.Boolean;
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Add"/> operation executor
        /// </summary>
        public static readonly DSharpAddInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Mod"/> operation
    /// </summary>
    public class DSharpModInstructionExecutor : DSharpMathInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override decimal PerformMathOperation(decimal left, decimal right)
        {
            return left % right;
        }
        protected override bool PerformMathOperation(bool left, bool right)
        {
            return false;
        }
        protected override bool CanPerform(DSharpStackValueType left, DSharpStackValueType right)
        {
            return left != DSharpStackValueType.Bool && right != DSharpStackValueType.Bool;
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Mod"/> operation executor
        /// </summary>
        public static readonly DSharpModInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

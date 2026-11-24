namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Jump"/> operation
    /// </summary>
    public unsafe class DSharpJumpInstructionExecutor : DSharpBaseJumpInstructionExecutor
    {
        #region Controls

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            InstanceExecute(instruction, ref context);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Jump"/> operation executor
        /// </summary>
        public static readonly DSharpJumpInstructionExecutor Instance = new();

        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            context.InstructionIndex = *(uint*)instruction->Arguments[0];
        }

        #endregion
    }
}

using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.JumpIfTrue"/> operation
    /// </summary>
    public unsafe sealed class DSharpJumpIfTrueInstructionExecutor : DSharpBaseJumpInstructionExecutor
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
        /// Global instance of <see cref="DSharpBytecodeOperation.JumpIfTrue"/> operation executor
        /// </summary>
        public static readonly DSharpJumpIfTrueInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            if (*context.Stack.FastStructureDataPeek<bool>())
            {
                context.InstructionIndex = *(uint*)instruction->Arguments[0];
            }
        }

        #endregion
    }
}

using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Finally"/> operation
    /// </summary>
    public unsafe class DSharpFinallyInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            InstanceExecute(instruction, ref context);
        }

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            return 0;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Finally"/> operation executor
        /// </summary>
        public static readonly DSharpFinallyInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            if (0 >= context.CurrentTryCatchFinallyId)
            {
                *context.Callback = context.ThrowExecutionException("Unable to call finally block: no any try-catch-finally blocks");
                return;
            }
            if (!context.TryGetCurrentFinallyBlockInstructionIndex(out uint finallyInstructionIndex))
            {
                *context.Callback = context.ThrowExecutionException("Unable to call finally block: current try-catch-finally block not contains finally block");
                return;
            }

            context.NextReturnInstructions->Add(context.InstructionIndex + 1);
            context.InstructionIndex = finallyInstructionIndex;
        }

        #endregion
    }
}

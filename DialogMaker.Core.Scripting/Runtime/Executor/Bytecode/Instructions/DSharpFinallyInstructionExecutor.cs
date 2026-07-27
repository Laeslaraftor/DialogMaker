using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Finally"/> operation
    /// </summary>
    public class DSharpFinallyInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (0 >= context.CurrentTryCatchFinallyId)
            {
                return context.ThrowExecutionException("Unable to call finally block: no any try-catch-finally blocks");
            }
            if (!context.TryGetCurrentFinallyBlockInstructionIndex(out uint finallyInstructionIndex))
            {
                return context.ThrowExecutionException("Unable to call finally block: current try-catch-finally block not contains finally block");
            }

            context.NextReturnInstructions->Add(context.InstructionIndex + 1);
            context.InstructionIndex = finallyInstructionIndex;

            return DSharpMethodExecutionCallback.Complete();
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public unsafe override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            return 0;
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Finally"/> operation executor
        /// </summary>
        public static readonly DSharpFinallyInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

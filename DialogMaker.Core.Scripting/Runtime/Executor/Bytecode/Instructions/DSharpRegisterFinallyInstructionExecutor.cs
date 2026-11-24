using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.RegisterFinally"/> operation
    /// </summary>
    public unsafe class DSharpRegisterFinallyInstructionExecutor : DSharpInstructionExecutor
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
            stream->Read<int>();
            return 1;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = stream->ReadSafePointer<int>();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.RegisterFinally"/> operation executor
        /// </summary>
        public static readonly DSharpRegisterFinallyInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (CheckArguments(instruction, ref context, 1))
            {
                return;
            }
#endif
            if (0 >= context.CurrentTryCatchFinallyId)
            {
                *context.Callback = context.ThrowExecutionException("Unable to register finally block with no any try-catch-finally block");
            }

            int instructionIndex = *(int*)instruction->Arguments[0];

            if (!context.AddFinallyBlock(instructionIndex))
            {
                *context.Callback = context.ThrowExecutionException("Current try-catch-finally block already contains finally block");
            }
        }

        #endregion
    }
}

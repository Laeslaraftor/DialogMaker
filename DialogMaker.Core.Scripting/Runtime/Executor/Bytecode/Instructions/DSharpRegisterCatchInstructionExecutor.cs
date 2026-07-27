using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.RegisterCatch"/> operation
    /// </summary>
    public class DSharpRegisterCatchInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (CheckArguments(instruction, context, 1, out var error))
            {
                return error;
            }
            if (0 >= context.CurrentTryCatchFinallyId)
            {
                return context.ThrowExecutionException("Unable to register catch block with no any try-catch-finally block");
            }

            int instructionIndex = *(int*)instruction.Arguments[0];

            if (!context.AddCatchBlock(null, instructionIndex))
            {
                return context.ThrowExecutionException("Current try-catch-finally block already contains catch block that handles all exceptions");
            }

            return DSharpMethodExecutionCallback.Complete();
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public unsafe override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            stream->Read<int>();
            return 1;
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = stream->ReadSafePointer<int>();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.RegisterCatch"/> operation executor
        /// </summary>
        public static readonly DSharpRegisterCatchInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.RegisterFinally"/> operation
    /// </summary>
    public class DSharpRegisterFinallyInstructionExecutor : DSharpInstructionExecutor
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
                return context.ThrowExecutionException("Unable to register finally block with no any try-catch-finally block");
            }

            int instructionIndex = *(int*)instruction.Arguments[0];

            if (!context.AddFinallyBlock(instructionIndex))
            {
                return context.ThrowExecutionException("Current try-catch-finally block already contains finally block");
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
        /// Global instance of <see cref="DSharpBytecodeOperation.RegisterFinally"/> operation executor
        /// </summary>
        public static readonly DSharpRegisterFinallyInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

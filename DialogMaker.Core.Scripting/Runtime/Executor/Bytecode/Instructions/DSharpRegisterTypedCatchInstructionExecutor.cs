using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.RegisterTypedCatch"/> operation
    /// </summary>
    public class DSharpRegisterTypedCatchInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (CheckArguments(instruction, context, 2, out var error))
            {
                return error;
            }
            if (0 >= context.CurrentTryCatchFinallyId)
            {
                return context.ThrowExecutionException("Unable to register catch block with no any try-catch-finally block");
            }

            int instructionIndex = *(int*)instruction.Arguments[0];
            var exceptionTypeToken = *(DSharpMetadataToken*)instruction.Arguments[1];
            DSharpRuntimeTypeInfo* exceptionType;

            try
            {
                exceptionType = context.GetType(exceptionTypeToken);
            }
            catch (Exception exception)
            {
                return context.ThrowExecutionException(exception);
            }

            if (!context.AddCatchBlock(exceptionType, instructionIndex))
            {
                return context.ThrowExecutionException($"Current try-catch-finally block already contains catch block that handles \"{exceptionType->ToString()}\"");
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
            stream->Read<DSharpMetadataToken>();
            return 2;
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = stream->ReadSafePointer<int>();
            arguments[1] = stream->ReadSafePointer<DSharpMetadataToken>();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.RegisterTypedCatch"/> operation executor
        /// </summary>
        public static readonly DSharpRegisterTypedCatchInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

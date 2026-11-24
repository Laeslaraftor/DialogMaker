using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.RegisterTypedCatch"/> operation
    /// </summary>
    public unsafe class DSharpRegisterTypedCatchInstructionExecutor : DSharpInstructionExecutor
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
            stream->Read<DSharpMetadataToken>();
            return 2;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = stream->ReadSafePointer<int>();
            var typeToken = stream->Read<DSharpMetadataToken>();

            arguments[1] = (nint)typesProvider.GetRuntimeInfo(typeToken);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.RegisterTypedCatch"/> operation executor
        /// </summary>
        public static readonly DSharpRegisterTypedCatchInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (CheckArguments(instruction, ref context, 2))
            {
                return;
            }
#endif
            if (0 >= context.CurrentTryCatchFinallyId)
            {
                *context.Callback = context.ThrowExecutionException("Unable to register catch block with no any try-catch-finally block");
                return;
            }

            int instructionIndex = *(int*)instruction->Arguments[0];
            var exceptionType = (DSharpRuntimeTypeInfo*)instruction->Arguments[1];
            exceptionType = context.ReplaceType(exceptionType);

            if (!context.AddCatchBlock(exceptionType, instructionIndex))
            {
                *context.Callback = context.ThrowExecutionException($"Current try-catch-finally block already contains catch block that handles \"{exceptionType->ToString()}\"");
            }
        }

        #endregion
    }
}

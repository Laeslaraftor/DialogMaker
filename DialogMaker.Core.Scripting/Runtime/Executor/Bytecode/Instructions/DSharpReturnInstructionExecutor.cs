using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Return"/> operation
    /// </summary>
    public unsafe class DSharpReturnInstructionExecutor : DSharpInstructionExecutor
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
        /// Global instance of <see cref="DSharpBytecodeOperation.Return"/> operation executor
        /// </summary>
        public static readonly DSharpReturnInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            var nextInstructions = context.NextReturnInstructions;

            if (nextInstructions->Count > 0)
            {
                var lastIndex = nextInstructions->Count - 1;
                context.InstructionIndex = (*nextInstructions)[lastIndex];
                nextInstructions->RemoveAt(lastIndex);
                context.EndTryCatchFinally();

                if (context.HaveUnhandledException)
                {
                    *context.Callback = DSharpMethodExecutionCallback.Throw(context.UnhandledException);
                    return;
                }
                if (context.NowClosingTryCatchFinallyBlock)
                {
                    *context.Callback = DSharpMethodExecutionCallback.Throw(null);
                    return;
                }

                context.Stack.PushReference(context.UnhandledException);

                return;
            }

            *context.Callback = DSharpMethodExecutionCallback.Return();
        }

        #endregion
    }
}

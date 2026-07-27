using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Return"/> operation
    /// </summary>
    public class DSharpReturnInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            var nextInstructions = context.NextReturnInstructions;

            if (nextInstructions->Count > 0)
            {
                var lastIndex = nextInstructions->Count - 1;
                context.InstructionIndex = (*nextInstructions)[lastIndex];
                nextInstructions->RemoveAt(lastIndex);
                context.EndTryCatchFinally();

                if (context.NowClosingTryCatchFinallyBlock)
                {
                    return DSharpMethodExecutionCallback.Throw(null);
                }

                return DSharpMethodExecutionCallback.Complete();
            }

            return DSharpMethodExecutionCallback.Return();
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
        /// Global instance of <see cref="DSharpBytecodeOperation.Return"/> operation executor
        /// </summary>
        public static readonly DSharpReturnInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

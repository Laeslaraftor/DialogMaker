using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Pop"/> operation
    /// </summary>
    public unsafe class DSharpPopInstructionExecutor : DSharpInstructionExecutor
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
        /// Global instance of <see cref="DSharpBytecodeOperation.Pop"/> operation executor
        /// </summary>
        public static readonly DSharpPopInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (0 >= context.Stack.Count)
            {
                *context.Callback = context.ThrowExecutionException("Unable to pop stack: stack is empty");
            }
#endif

            context.Stack.PopCount(1);
        }

        #endregion
    }
}

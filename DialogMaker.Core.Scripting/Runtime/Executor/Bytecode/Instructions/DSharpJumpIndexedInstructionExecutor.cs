using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.JumpIndexed"/> operation
    /// </summary>
    public unsafe class DSharpJumpIndexedInstructionExecutor : DSharpInstructionExecutor
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
        /// Global instance of <see cref="DSharpBytecodeOperation.JumpIndexed"/> operation executor
        /// </summary>
        public static readonly DSharpJumpIndexedInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (CheckStackValues(instruction, ref context, 1))
            {
                return;
            }
#endif

            var value = context.Stack.Peek();

#if DEBUG
            if (!value.IsNumber)
            {
                *context.Callback = context.ThrowExecutionException($"Jumping by index that placed in stack requires number value, got: {value.ValueType}");
                return;
            }
#endif

            context.InstructionIndex = value.ReadAsNumber().Convert<uint>();
        }

        #endregion
    }
}

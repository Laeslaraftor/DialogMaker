using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Not"/> operation
    /// </summary>
    public unsafe class DSharpNotInstructionExecutor : DSharpInstructionExecutor
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
        /// Global instance of <see cref="DSharpBytecodeOperation.Not"/> operation executor
        /// </summary>
        public static readonly DSharpNotInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (CheckStackValues(instruction, ref context, 1))
            {
                return;
            }

            var value = context.Stack.Peek();
            var obj = value.ReadAsObject();
            var data = DSharpObject.GetData<bool>(obj);

            if (obj == null || obj->Type != context.TypesProvider.Boolean)
            {
                *context.Callback = context.ThrowExecutionException($"Unable to invert value because it is not boolean: {value.ValueType}");
                return;
            }
#else
            var data = context.Stack.FastStructureDataPeek<bool>();
#endif

            *data = !*data;
        }

        #endregion
    }
}

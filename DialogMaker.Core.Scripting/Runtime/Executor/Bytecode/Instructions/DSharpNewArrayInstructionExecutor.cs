using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.NewArray"/> operation
    /// </summary>
    public unsafe class DSharpNewArrayInstructionExecutor : DSharpTypeInstructionExecutor
    {
        #region Controls

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, DSharpRuntimeTypeInfo* runtimeInfo)
        {
            Create(instruction, ref context, runtimeInfo, false);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.NewArray"/> operation executor
        /// </summary>
        public static readonly DSharpNewArrayInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Create(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, DSharpRuntimeTypeInfo* arrayType, bool isStackAlloc)
        {
#if DEBUG
            if (CheckStackValues(instruction, ref context, 1))
            {
                return;
            }
#endif

            int length;

            try
            {
                var lastValue = context.Stack.Peek();
                length = (int)lastValue.ReadAsDecimal().GetValueOrDefault();
            }
            catch (Exception exception)
            {
                *context.Callback = context.ThrowExecutionException($"Unable to get array length: {exception}");
                return;
            }

            DSharpObject* arrayInstance;

            try
            {
                arrayInstance = context.ObjectsContainer.CreateArray(arrayType, length, isStackAlloc ? context.Stack : null);
            }   
            catch (Exception exception)
            {
                *context.Callback = context.ThrowExecutionException(exception);
                return;
            }

            if (!isStackAlloc)
            {
                context.Stack.PushReference(arrayInstance);
            }
        }

        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.StoreField"/> operation
    /// </summary>
    public unsafe class DSharpStoreFieldInstructionExecutor : DSharpFieldInstructionExecutor
    {
        #region Controls

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, DSharpRuntimeFieldInfo* runtimeInfo)
        {
            Store(instruction, ref context, runtimeInfo, false);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.StoreField"/> operation executor
        /// </summary>
        public static readonly DSharpStoreFieldInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Store(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, DSharpRuntimeFieldInfo* field, bool isInstance)
        {
            int stackValuesCount = isInstance ? 2 : 1;

#if DEBUG
            if (CheckStackValues(instruction, ref context, stackValuesCount))
            {
                return;
            }
#endif

            DSharpObject* instance = null;

            if (isInstance)
            {
                instance = GetInstance(ref context, (uint)stackValuesCount - 1, throwOnError: true);

                if (instance == null)
                {
                    return;
                }
            }

            try
            {
                field->Write(context.ObjectsContainer, instance, context.Stack, 0);
            }
            catch (Exception exception)
            {
                *context.Callback = context.ThrowExecutionException(exception);
                return;
            }
        }

        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

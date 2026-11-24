using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.LoadField"/> operation
    /// </summary>
    public unsafe class DSharpLoadFieldInstructionExecutor : DSharpFieldInstructionExecutor
    {
        #region Controls

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, DSharpRuntimeFieldInfo* runtimeInfo)
        {
            Load(instruction, ref context, runtimeInfo, false);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.LoadField"/> operation executor
        /// </summary>
        public static readonly DSharpLoadFieldInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Load(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, DSharpRuntimeFieldInfo* field, bool isInstance)
        {
            DSharpObject* instance = null;

            if (isInstance)
            {
#if DEBUG
                if (CheckStackValues(instruction, ref context, 1))
                {
                    return;
                }
#endif

                instance = GetInstance(ref context, 0, throwOnError: true);

                if (instance == null)
                {
                    return;
                }
            }

            try
            {
                field->Read(instance, context.Stack);
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

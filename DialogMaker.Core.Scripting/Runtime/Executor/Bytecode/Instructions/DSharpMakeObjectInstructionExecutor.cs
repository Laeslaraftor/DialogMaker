using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.MakeObject"/> operation
    /// </summary>
    public unsafe class DSharpMakeObjectInstructionExecutor : DSharpTypeInstructionExecutor
    {
        #region Controls

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, DSharpRuntimeTypeInfo* type)
        {
#if DEBUG
            if (CheckStackValues(instruction, ref context, 1))
            {
                return;
            }
#endif

            var lastValue = context.Stack.Peek();
            var addressInstance = lastValue.ReadAsObject();
            nint dataAddress = DSharpObjectConverter.ToIntPtr(addressInstance);

            if (dataAddress == 0)
            {
                context.Stack.PushNull();
            }
            else
            {
                context.Stack.PushRedirectObject(type, dataAddress);
            }
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.MakeObject"/> operation executor
        /// </summary>
        public static readonly DSharpMakeObjectInstructionExecutor Instance = new();

        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

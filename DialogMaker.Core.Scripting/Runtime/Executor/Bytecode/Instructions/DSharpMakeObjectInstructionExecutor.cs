using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.MakeObject"/> operation
    /// </summary>
    public class DSharpMakeObjectInstructionExecutor : DSharpTypeInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimeTypeInfo* type)
        {
            if (CheckStackValues(instruction, context, 1, out var error))
            {
                return error;
            }

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

            return DSharpMethodExecutionCallback.Complete();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.MakeObject"/> operation executor
        /// </summary>
        public static readonly DSharpMakeObjectInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

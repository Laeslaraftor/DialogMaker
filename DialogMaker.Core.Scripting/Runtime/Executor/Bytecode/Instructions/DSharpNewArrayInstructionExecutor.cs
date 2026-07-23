using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.NewArray"/> operation
    /// </summary>
    public class DSharpNewArrayInstructionExecutor : DSharpMetadataTokenInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected unsafe override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken)
        {
            if (CheckStackValues(instruction, context, 1, out var error))
            {
                return error;
            }

            int length;

            try
            {
                var lastValue = context.Stack.Peek();
                length = (int)lastValue.ReadAsDecimal().GetValueOrDefault();
            }
            catch (Exception exception)
            {
                return context.ThrowExecutionException($"Unable to get array length: {exception}");
            }

            DSharpRuntimeTypeInfo* arrayType;

            try
            {
                arrayType = context.TypesProvider.GetRuntimeInfo(metadataToken);
            }
            catch (Exception exception)
            {
                return context.ThrowExecutionException(exception);
            }

            var arrayInstance = context.ObjectsContainer.CreateArray(arrayType, length);
            context.Stack.PushReference(arrayInstance);

            return DSharpMethodExecutionCallback.Complete();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.NewArray"/> operation executor
        /// </summary>
        public static readonly DSharpNewArrayInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

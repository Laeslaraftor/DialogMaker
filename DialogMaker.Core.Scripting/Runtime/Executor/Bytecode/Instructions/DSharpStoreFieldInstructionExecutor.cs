using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.StoreField"/> operation
    /// </summary>
    public class DSharpStoreFieldInstructionExecutor : DSharpMetadataTokenInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken)
        {
            return Store(instruction, ref context, metadataToken, false);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.StoreField"/> operation executor
        /// </summary>
        public static readonly DSharpStoreFieldInstructionExecutor Instance = new();

        internal static unsafe DSharpMethodExecutionCallback Store(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken, bool isInstance)
        {
            int stackValuesCount = isInstance ? 2 : 1;

            if (CheckStackValues(instruction, context, stackValuesCount, out var error))
            {
                return error;
            }

            DSharpObject* instance = null;

            if (isInstance)
            {
                instance = GetInstance(context, 1, out error);

                if (instance == null)
                {
                    return error;
                }
            }

            try
            {
                var field = context.TypesProvider.GetField(metadataToken);
                field->Write(context.ObjectsContainer, instance, context.Stack);
            }
            catch (Exception exception)
            {
                return context.ThrowExecutionException(exception);
            }

            return DSharpMethodExecutionCallback.Complete();
        }

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

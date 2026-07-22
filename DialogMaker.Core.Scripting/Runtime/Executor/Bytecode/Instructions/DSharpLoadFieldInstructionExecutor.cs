namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.LoadField"/> operation
    /// </summary>
    public class DSharpLoadFieldInstructionExecutor : DSharpMetadataTokenInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken)
        {
            return Load(instruction, ref context, metadataToken, false);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.LoadField"/> operation executor
        /// </summary>
        public static readonly DSharpLoadFieldInstructionExecutor Instance = new();

        internal static unsafe DSharpMethodExecutionCallback Load(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken, bool isInstance)
        {
            DSharpObject* instance = null;

            if (isInstance)
            {
                if (CheckStackValues(instruction, context, 1, out var error))
                {
                    return error;
                }

                instance = GetInstance(context, 0, out error);

                if (instance == null)
                {
                    return error;
                }
            }

            try
            {
                var field = context.TypesProvider.GetField(metadataToken);
                field->Read(instance, context.Stack);
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

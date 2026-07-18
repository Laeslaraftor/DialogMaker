using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.LoadInstanceField"/> operation
    /// </summary>
    public class DSharpLoadInstanceFieldInstructionExecutor : DSharpMetadataTokenInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken)
        {
            if (CheckStackValues(instruction, context, 1, out var error))
            {
                return error;
            }

            return TryGetInstanceFromStack(ref context, (ref context, instance) =>
            {
                if (!instance->Type->TryGetField(metadataToken, out var field))
                {
                    return context.ThrowExecutionException($"Unable to get field for token: {metadataToken}");
                }

                field->Read(instance, context.Stack);

                return DSharpMethodExecutionCallback.Complete();
            });
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.LoadInstanceField"/> operation executor
        /// </summary>
        public static readonly DSharpLoadInstanceFieldInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

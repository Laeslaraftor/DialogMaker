namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.New"/> operation
    /// </summary>
    public class DSharpNewInstructionExecutor : DSharpMetadataTokenInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken)
        {
            IDSharpMemberInfo member;

            try
            {
                member = context.TypesProvider.Assembly.GetType(metadataToken);
            }
            catch (Exception error)
            {
                return context.ThrowExecutionException(error);
            }

            IDSharpType typeToInstantiate;
            IDSharpMethodInfo? constructor = null;

            if (member is IDSharpType typeMember)
            {
                typeToInstantiate = typeMember;
            }
            else if (member is IDSharpMethodInfo methodMember)
            {
                typeToInstantiate = methodMember.DeclaringType;
                constructor = methodMember;
            }

            if (constructor != null)
            {

            }

            return DSharpMethodExecutionCallback.Complete();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.New"/> operation executor
        /// </summary>
        public static readonly DSharpNewInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Call"/> operation
    /// </summary>
    public class DSharpCallInstructionExecutor : DSharpMetadataTokenInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected unsafe override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken)
        {
            var method = context.TypesProvider.GetStaticMethod(metadataToken);
            var parametersCount = method->ParametersType.Length;

            if (CheckStackValues(instruction, context, parametersCount, out var error))
            {
                return error;
            }

            var argsFrame = context.Stack.Push(DSharpStackValueType.MethodParametersBuffer, sizeof(DSharpExecutionLocalVariable) * parametersCount);
            UnmanagedArray<DSharpExecutionLocalVariable> arguments = new(argsFrame.StackPointer, parametersCount);

            for (int i = 0; i < parametersCount; i++)
            {
                arguments[i] = new()
                {
                    ParameterInfo = method->ParametersType.GetItemReference(i),
                    Buffer = context.Stack.Peek((uint)(parametersCount - i))
                };
            }

            return DSharpMethodExecutionCallback.Call(null, method, arguments);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Call"/> operation executor
        /// </summary>
        public static readonly DSharpCallInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

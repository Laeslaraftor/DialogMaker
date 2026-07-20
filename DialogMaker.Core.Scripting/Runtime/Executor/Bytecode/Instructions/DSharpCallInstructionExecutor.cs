using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using static DialogMaker.Core.Scripting.Compiler.Builders.DSharpBytecodeBuilder;

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
            var method = context.TypesProvider.GetMethod(metadataToken);
            var parametersCount = method->ParametersType.Length;

            if (CheckStackValues(instruction, context, parametersCount, out var error))
            {
                return error;
            }

            var arguments = CreateArguments(context, method, 0);

            return DSharpMethodExecutionCallback.Call(null, method, arguments);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Call"/> operation executor
        /// </summary>
        public static readonly DSharpCallInstructionExecutor Instance = new();

        internal static unsafe UnmanagedArray<DSharpExecutionLocalVariable> CreateArguments(DSharpExecutionContext context, DSharpRuntimeMethodInfo* methodInfo, uint offset = 0)
        {
            if (methodInfo->ParametersType.IsNull)
            {
                return new(0, 0);
            }

            var parametersCount = methodInfo->ParametersType.Length;
            var argsFrame = *context.Stack.Push(DSharpStackValueType.MethodParametersBuffer, sizeof(DSharpExecutionLocalVariable) * parametersCount);
            UnmanagedArray<DSharpExecutionLocalVariable> arguments = new(argsFrame.StackPointer, parametersCount);

            for (int i = 0; i < parametersCount; i++)
            {
                arguments[i] = new()
                {
                    ParameterInfo = methodInfo->ParametersType.GetItemReference(i),
                    Buffer = context.Stack.Peek((uint)(parametersCount - i) + offset)
                };
            }

            return arguments;
        }

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

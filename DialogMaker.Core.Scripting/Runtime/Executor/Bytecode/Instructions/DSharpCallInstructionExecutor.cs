using Acly.Commands;
using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

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
            return Call(instruction, ref context, metadataToken, false, false);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Call"/> operation executor
        /// </summary>
        public static readonly DSharpCallInstructionExecutor Instance = new();

        internal static unsafe DSharpMethodExecutionCallback Call(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken, bool isInstance, bool isBase)
        {
            var method = context.TypesProvider.GetMethod(metadataToken);
            var parametersCount = method->ParametersType.Length;

            if (isInstance)
            {
                parametersCount++;
            }
            if (CheckStackValues(instruction, context, parametersCount, out var error))
            {
                return error;
            }

            DSharpObject* instance = null;

            if (isInstance)
            {
                instance = GetInstance(context, (uint)parametersCount - 1, out error);

                if (instance == null)
                {
                    return error;
                }
                if (!isBase && method->CanBeOverriden)
                {
                    if (instance->Type->OverridenMethods.TryGetValue(method, out var endPointMethod))
                    {
                        method = endPointMethod;
                    }
                    else if (method->DeclaringType->ObjectType == DSharpObjectType.Interface ||
                             method->IsAbstract)
                    {
                        return context.ThrowExecutionException($"Unable to find end-point method for \"{method->ToString()}\"");
                    }
                }
            }

            var arguments = CreateArguments(context, method, 0);

            return DSharpMethodExecutionCallback.Call(instance, method, arguments);
        }
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
                var peekOffset = (uint)(parametersCount - i) + offset;
                var frame = context.Stack.Peek(peekOffset);
                arguments[i] = new()
                {
                    ParameterInfo = methodInfo->ParametersType.GetItemReference(i),
                    Buffer = frame
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

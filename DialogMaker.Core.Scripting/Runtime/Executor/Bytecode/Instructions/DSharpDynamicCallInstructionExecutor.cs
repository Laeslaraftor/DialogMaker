using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.DynamicCall"/> operation
    /// </summary>
    public class DSharpDynamicCallInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public unsafe override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            var methodInfoAddressInstance = GetInstance(context, 0, out var error);

            if (methodInfoAddressInstance == null)
            {
                return context.ThrowExecutionException("Invalid runtime method information address");
            }

            var methodInfo = (DSharpRuntimeMethodInfo*)DSharpObjectConverter.ToIntPtr(methodInfoAddressInstance);

            if (methodInfo->GenericTypes.Length > 0)
            {
                return context.ThrowExecutionException("Dynamic calling not supported generic methods");
            }

            var instance = GetInstance(context, (uint)methodInfo->ParametersType.Length + 1, out error);

            return DSharpCallInstructionExecutor.Call(instruction, ref context, new()
            {
                ArgumentsCount = (uint)methodInfo->ParametersType.Length,
                CallingType = DSharpMethodCallingType.Virtual,
                Instance = instance,
                Member = methodInfo
            }, argumentsOffset: 1);
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public unsafe override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            return 0;
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.DynamicCall"/> operation executor
        /// </summary>
        public static readonly DSharpDynamicCallInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

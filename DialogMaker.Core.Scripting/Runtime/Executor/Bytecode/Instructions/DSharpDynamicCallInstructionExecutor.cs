using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.DynamicCall"/> operation
    /// </summary>
    public unsafe class DSharpDynamicCallInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            InstanceExecute(instruction, ref context);
        }

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            return 0;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.DynamicCall"/> operation executor
        /// </summary>
        public static readonly DSharpDynamicCallInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            var methodInfoAddressInstance = GetInstance(ref context, 0);

            if (methodInfoAddressInstance == null)
            {
                *context.Callback = context.ThrowExecutionException("Invalid runtime method information address");
                return;
            }

            var methodInfo = (DSharpRuntimeMethodInfo*)DSharpObjectConverter.ToIntPtr(methodInfoAddressInstance);

            if (methodInfo->GenericTypes.Length > 0)
            {
                *context.Callback = context.ThrowExecutionException("Dynamic calling not supported generic methods");
                return;
            }

            var instance = GetInstance(ref context, (uint)methodInfo->ParametersType.Length + 1);

            DSharpCallInstructionExecutor.Call(instruction, ref context, new()
            {
                ArgumentsCount = (uint)methodInfo->ParametersType.Length,
                CallingType = DSharpMethodCallingType.Virtual,
                Instance = instance,
                Member = methodInfo
            }, argumentsOffset: 1);
        }

        #endregion
    }
}

using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.ReadOnAddress"/> operation
    /// </summary>
    public class DSharpReadOnAddressInstructionExecutor : DSharpTypeInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimeTypeInfo* runtimeInfo)
        {
            var instance = GetInstance(context, 0, out var error);

            if (instance == null)
            {
                return error;
            }
            if (instance->Type != context.TypesProvider.IntPtr)
            {
                return context.ThrowExecutionException($"Unable to read value on address: invalid address value, required \"{context.TypesProvider.IntPtr->ToString()}\", but got \"{instance->Type->ToString()}\"");
            }

            var data = *(nint*)DSharpObject.GetData(instance);
            UnmanagedArray<byte> buffer = new(data, runtimeInfo->Size);

            context.Stack.PushStructure(runtimeInfo, buffer);

            return DSharpMethodExecutionCallback.Complete();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.ReadOnAddress"/> operation executor
        /// </summary>
        public static readonly DSharpReadOnAddressInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

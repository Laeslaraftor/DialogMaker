using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.StoreOnAddress"/> operation
    /// </summary>
    public class DSharpStoreOnAddressInstructionExecutor : DSharpTypeInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimeTypeInfo* runtimeInfo)
        {
            var addressInstance = GetInstance(context, 0, out var error);
            var valueInstance = GetInstance(context, 1, out error);

            if (addressInstance == null)
            {
                return error;
            }
            if (addressInstance->Type != context.TypesProvider.IntPtr)
            {
                return context.ThrowExecutionException($"Unable to read value on address: invalid address value, required \"{context.TypesProvider.IntPtr->ToString()}\", but got \"{addressInstance->Type->ToString()}\"");
            }

            var pointer = (void*)*(nint*)DSharpObject.GetData(addressInstance);

            if (valueInstance == null)
            {
                RuntimeExtensions.FillZero(pointer, runtimeInfo->Size);
            }
            else
            {
                int size = runtimeInfo->Size;
                var valueData = DSharpObject.GetData(valueInstance);
                Buffer.MemoryCopy(valueData, pointer, size, Math.Min(size, valueInstance->Type->Size));
            }

            return DSharpMethodExecutionCallback.Complete();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.StoreOnAddress"/> operation executor
        /// </summary>
        public static readonly DSharpStoreOnAddressInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

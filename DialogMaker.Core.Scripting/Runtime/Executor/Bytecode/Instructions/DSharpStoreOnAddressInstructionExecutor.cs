using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.StoreOnAddress"/> operation
    /// </summary>
    public unsafe class DSharpStoreOnAddressInstructionExecutor : DSharpTypeInstructionExecutor
    {
        #region Controls

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, DSharpRuntimeTypeInfo* runtimeInfo)
        {
            var addressInstance = GetInstance(ref context, 0, throwOnError: true);

            if (addressInstance == null)
            {
                return;
            }

            var valueInstance = GetInstance(ref context, 1);

#if DEBUG
            if (addressInstance->Type != context.TypesProvider.IntPtr)
            {
                *context.Callback = context.ThrowExecutionException($"Unable to read value on address: invalid address value, required \"{context.TypesProvider.IntPtr->ToString()}\", but got \"{addressInstance->Type->ToString()}\"");
                return;
            }
#endif

            var pointer = (void*)*(nint*)DSharpObject.GetData(addressInstance);

            if (runtimeInfo->IsValueType)
            {
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
            }
            else
            {
                *(DSharpObject**)pointer = valueInstance;
            }
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.StoreOnAddress"/> operation executor
        /// </summary>
        public static readonly DSharpStoreOnAddressInstructionExecutor Instance = new();

        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

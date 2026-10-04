using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.StoreFieldOnAddress"/> operation
    /// </summary>
    public class DSharpStoreFieldOnAddressInstructionExecutor : DSharpFieldInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimeFieldInfo* runtimeInfo)
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

            var pointerAddress = *(nint*)DSharpObject.GetData(addressInstance);
            var fieldOffset = DSharpRuntimeFieldInfo.GetOffset(runtimeInfo);
            var fieldType = runtimeInfo->FieldType;
            var pointer = (void*)(pointerAddress + fieldOffset);

            if (valueInstance == null)
            {
                RuntimeExtensions.FillZero(pointer, fieldType->Size);
            }
            else
            {
                int size = fieldType->Size;
                var valueData = DSharpObject.GetData(valueInstance);
                Buffer.MemoryCopy(valueData, pointer, size, Math.Min(size, valueInstance->Type->Size));
            }

            return DSharpMethodExecutionCallback.Complete();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.StoreFieldOnAddress"/> operation executor
        /// </summary>
        public static readonly DSharpStoreFieldOnAddressInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

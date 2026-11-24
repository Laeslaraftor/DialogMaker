using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.ReadOnAddress"/> operation
    /// </summary>
    public unsafe class DSharpReadOnAddressInstructionExecutor : DSharpTypeInstructionExecutor
    {
        #region Controls

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, DSharpRuntimeTypeInfo* runtimeInfo)
        {
            var instance = GetInstance(ref context, 0, throwOnError: true);

            if (instance == null)
            {
                return;
            }
            if (instance->Type != context.TypesProvider.IntPtr)
            {
                *context.Callback = context.ThrowExecutionException($"Unable to read value on address: invalid address value, required \"{context.TypesProvider.IntPtr->ToString()}\", but got \"{instance->Type->ToString()}\"");
                return;
            }
            if (runtimeInfo->IsValueType)
            {
                var data = *(nint*)DSharpObject.GetData(instance);
                UnmanagedArray<byte> buffer = new(data, runtimeInfo->Size);

                context.Stack.PushStructure(runtimeInfo, buffer);
            }
            else
            {
                // 1. Pointer to structure data that stores System.IntPtr
                // 2. Pointer to instance pointer (previous System.IntPtr)
                // 3. Pointer to D# object instance
                var obj = **(DSharpObject***)DSharpObject.GetData(instance); // обсидиановая фигня

                if (obj == null)
                {
                    context.Stack.PushNull();
                }
                else
                {
                    context.Stack.PushReference(obj, true);
                }
            }
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.ReadOnAddress"/> operation executor
        /// </summary>
        public static readonly DSharpReadOnAddressInstructionExecutor Instance = new();

        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

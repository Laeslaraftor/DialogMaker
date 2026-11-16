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

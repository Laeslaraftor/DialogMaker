using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.NewArray"/> operation
    /// </summary>
    public class DSharpNewArrayInstructionExecutor : DSharpTypeInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimeTypeInfo* runtimeInfo)
        {
            return Create(instruction, ref context, runtimeInfo, false);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.NewArray"/> operation executor
        /// </summary>
        public static readonly DSharpNewArrayInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe DSharpMethodExecutionCallback Create(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimeTypeInfo* arrayType, bool isStackAlloc)
        {
            if (CheckStackValues(instruction, context, 1, out var error))
            {
                return error;
            }

            int length;

            try
            {
                var lastValue = context.Stack.Peek();
                length = (int)lastValue.ReadAsDecimal().GetValueOrDefault();
            }
            catch (Exception exception)
            {
                return context.ThrowExecutionException($"Unable to get array length: {exception}");
            }

            DSharpObject* arrayInstance;

            try
            {
                arrayInstance = context.ObjectsContainer.CreateArray(arrayType, length, isStackAlloc ? context.Stack : null);
            }   
            catch (Exception exception)
            {
                return context.ThrowExecutionException(exception);
            }

            if (!isStackAlloc)
            {
                context.Stack.PushReference(arrayInstance);
            }

            return DSharpMethodExecutionCallback.Complete();

        }

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

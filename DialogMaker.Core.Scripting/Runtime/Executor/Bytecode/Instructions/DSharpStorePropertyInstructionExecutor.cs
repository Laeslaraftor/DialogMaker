using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.StoreProperty"/> operation
    /// </summary>
    public class DSharpStorePropertyInstructionExecutor : DSharpPropertyInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimePropertyInfo* runtimeInfo)
        {
            return Store(instruction, ref context, runtimeInfo, false, false);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.StoreProperty"/> operation executor
        /// </summary>
        public static readonly DSharpStorePropertyInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe DSharpMethodExecutionCallback Store(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimePropertyInfo* property, bool isInstance, bool isBase)
        {
            return CallAccessor(instruction, ref context, property, DSharpPropertyAccessor.Setter, isInstance, isBase);
        }

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.GenericCallInstance"/> operation
    /// </summary>
    public class DSharpGenericCallInstanceInstructionExecutor : DSharpGenericCallInstructionExecutorBase
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimeMethodInfo* methodToken, UnmanagedArray<Pointer<DSharpRuntimeTypeInfo>> genericParameters)
        {
            return DSharpCallInstructionExecutor.Call(instruction, ref context, methodToken, true, false, genericParameters);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.GenericCallInstance"/> operation executor
        /// </summary>
        public static readonly DSharpGenericCallInstanceInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

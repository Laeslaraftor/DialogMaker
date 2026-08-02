using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.GenericCallBaseInstance"/> operation
    /// </summary>
    public class DSharpGenericCallBaseInstanceInstructionExecutor : DSharpGenericCallInstructionExecutorBase
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimeMethodInfo* methodToken, UnmanagedArray<Pointer<DSharpRuntimeTypeInfo>> genericParameters)
        {
            return DSharpCallInstructionExecutor.Call(instruction, ref context, methodToken, true, true, genericParameters);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.GenericCallBaseInstance"/> operation executor
        /// </summary>
        public static readonly DSharpGenericCallBaseInstanceInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

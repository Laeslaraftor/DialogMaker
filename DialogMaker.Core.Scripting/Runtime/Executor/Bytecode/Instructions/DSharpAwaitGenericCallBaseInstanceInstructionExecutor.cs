using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.AwaitGenericCallBaseInstance"/> operation
    /// </summary>
    public class DSharpAwaitGenericCallBaseInstanceInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream)
        {
            throw new NotImplementedException();
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream, UnmanagedArray<nint> arguments)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.AwaitGenericCallBaseInstance"/> operation executor
        /// </summary>
        public static readonly DSharpAwaitGenericCallBaseInstanceInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

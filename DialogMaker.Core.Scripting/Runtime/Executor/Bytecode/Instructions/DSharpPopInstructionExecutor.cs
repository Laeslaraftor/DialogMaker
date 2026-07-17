using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Pop"/> operation
    /// </summary>
    public class DSharpPopInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (0 >= context.Stack.Count)
            {
                return context.ThrowExecutionException("Unable to pop stack: stack is empty");
            }

            context.Stack.Pop();

            return DSharpMethodExecutionCallback.Complete();
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream)
        {
            return 0;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream, UnmanagedArray<nint> arguments)
        {
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Pop"/> operation executor
        /// </summary>
        public static readonly DSharpPopInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

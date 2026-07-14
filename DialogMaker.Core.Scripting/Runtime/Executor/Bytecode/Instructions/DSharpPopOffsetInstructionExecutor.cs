using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.PopOffset"/> operation
    /// </summary>
    public class DSharpPopOffsetInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override unsafe bool Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (!CheckArguments(instruction, context, 1))
            {
                return false;
            }

            uint offset = *(uint*)instruction.Arguments[0];
            context.Stack.Pop(offset);
            return true;
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, bool> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream)
        {
            stream.Read<uint>();
            return 1;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = stream.ReadSafePointer<uint>();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.PopOffset"/> operation executor
        /// </summary>
        public static readonly DSharpPopOffsetInstructionExecutor Instance = new();

        private static bool InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

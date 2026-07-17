using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.LoadTypeSize"/> operation
    /// </summary>
    public class DSharpLoadTypeSizeInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (CheckArguments(instruction, context, 1, out var error))
            {
                return error;
            }

            var token = *(DSharpMetadataToken*)instruction.Arguments[0];
            var typeInfo = context.TypesProvider.GetRuntimeInfo(token);

            context.Stack.Push(typeInfo->Size);

            return DSharpMethodExecutionCallback.Complete();
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream)
        {
            stream.Read<DSharpMetadataToken>();
            return 1;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = stream.ReadSafePointer<DSharpMetadataToken>();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.LoadTypeSize"/> operation executor
        /// </summary>
        public static readonly DSharpLoadTypeSizeInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

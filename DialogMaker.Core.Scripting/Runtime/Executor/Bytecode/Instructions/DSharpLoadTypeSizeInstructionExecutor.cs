using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.LoadTypeSize"/> operation
    /// </summary>
    public unsafe class DSharpLoadTypeSizeInstructionExecutor : DSharpTypeInstructionExecutor
    {
        #region Controls

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            base.GetArgumentsCount(typesProvider, stream);
            stream->Read<DSharpLoadTypeSizeMode>();

            return 2;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            base.ReadArguments(typesProvider, stream, arguments);
            arguments[1] = (nint)stream->Read<DSharpLoadTypeSizeMode>();
        }

        protected override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, DSharpRuntimeTypeInfo* runtimeInfo)
        {
            var mode = (DSharpLoadTypeSizeMode)instruction->Arguments[1];

            if (mode == DSharpLoadTypeSizeMode.TotalSize)
            {
                context.Stack.Push(runtimeInfo->Size);
            }
            else
            {
                context.Stack.Push(runtimeInfo->ItemSize);
            }

            return DSharpMethodExecutionCallback.Complete();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.LoadTypeSize"/> operation executor
        /// </summary>
        public static readonly DSharpLoadTypeSizeInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

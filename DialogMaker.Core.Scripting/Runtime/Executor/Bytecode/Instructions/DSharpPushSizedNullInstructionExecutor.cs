using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.PushSizedNull"/> operation
    /// </summary>
    public class DSharpPushSizedNullInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (CheckArguments(instruction, context, 1, out var error))
            {
                return error;
            }

            var type = (DSharpRuntimeTypeInfo*)instruction.Arguments[0];
            type = context.ReplaceType(type);
            context.Stack.PushTypedNull(type);

            return DSharpMethodExecutionCallback.Complete();
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public unsafe override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            stream->Read<DSharpMetadataToken>();
            return 1;
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            var typeToken = *stream->ReadPointer<DSharpMetadataToken>();
            arguments[0] = (nint)typesProvider.GetRuntimeInfo(typeToken);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.PushSizedNull"/> operation executor
        /// </summary>
        public static readonly DSharpPushSizedNullInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

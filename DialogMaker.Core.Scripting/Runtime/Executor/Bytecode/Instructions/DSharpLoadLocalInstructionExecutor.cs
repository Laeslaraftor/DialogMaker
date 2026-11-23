using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.LoadLocal"/> operation
    /// </summary>
    public unsafe class DSharpLoadLocalInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (CheckArguments(instruction, context, 1, out var error))
            {
                return error;
            }
#endif

            uint variableIndex = *(uint*)instruction->Arguments[0];
            var variables = context.LocalVariables;

#if DEBUG
            if (variableIndex >= variables->Length)
            {
                return context.ThrowExecutionException($"Invalid local variable index \"{variableIndex}\" when available variables \"{variables->Length}\"");
            }
#endif

            var variable = variables->GetItemReference((int)variableIndex);
            var variableBuffer = variable->Buffer;
            var frame = context.Stack.Push(variableBuffer.ValueType, variableBuffer.Size);

            frame->Mode = variableBuffer.Mode;
            frame->Write(ref variableBuffer);

            return DSharpMethodExecutionCallback.Complete();
        }

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            stream->Read<uint>();
            return 1;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = stream->ReadSafePointer<uint>();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.LoadLocal"/> operation executor
        /// </summary>
        public static readonly DSharpLoadLocalInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

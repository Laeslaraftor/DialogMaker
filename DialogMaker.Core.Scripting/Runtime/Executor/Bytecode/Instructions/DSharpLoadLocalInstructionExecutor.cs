using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.LoadLocal"/> operation
    /// </summary>
    public unsafe class DSharpLoadLocalInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            InstanceExecute(instruction, ref context);
        }

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (CheckArguments(instruction, ref context, 1))
            {
                return;
            }
#endif

            uint variableIndex = *(uint*)instruction->Arguments[0];
            var variables = context.LocalVariables;

#if DEBUG
            if (variableIndex >= variables->Length)
            {
                *context.Callback = context.ThrowExecutionException($"Invalid local variable index \"{variableIndex}\" when available variables \"{variables->Length}\"");
                return;
            }
#endif

            var variable = variables->GetItemReference((int)variableIndex);
            var variableBuffer = variable->Buffer;
            var frame = context.Stack.Push(variableBuffer.ValueType, variableBuffer.Size);

            frame->Mode = variableBuffer.Mode;
            frame->Write(ref variableBuffer);
        }

        #endregion
    }
}

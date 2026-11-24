using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Push"/> operation
    /// </summary>
    public unsafe class DSharpPushInstructionExecutor : DSharpInstructionExecutor
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
            DSharpLiteralValue.Read(stream);
            return 1;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = stream->ReadSafePointer<DSharpLiteralType>();
            stream->Position -= sizeof(DSharpLiteralType);
            DSharpLiteralValue.Read(stream);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Push"/> operation executor
        /// </summary>
        public static readonly DSharpPushInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (CheckArguments(instruction, ref context, 1))
            {
                return;
            }
#endif

            var type = (DSharpLiteralType)(*(byte*)instruction->Arguments[0]);
            var valuePointer = instruction->Arguments[0] + sizeof(DSharpLiteralType);

            if (type == DSharpLiteralType.String)
            {
                UnmanagedStream stream = new(valuePointer, int.MaxValue);
                int length = stream.Read<int>();
                char* chars = stream.ReadPointer<char>();

                var str = context.ObjectsContainer.CreateString(chars, length);
                context.Stack.PushReference(str);
            }
            else
            {
                context.Stack.Push(type, valuePointer);
            }
        }

        #endregion
    }
}

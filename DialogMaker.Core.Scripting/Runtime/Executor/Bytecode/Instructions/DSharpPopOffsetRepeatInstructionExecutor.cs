using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.PopOffsetRepeat"/> operation
    /// </summary>
    public class DSharpPopOffsetRepeatInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override unsafe bool Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (!CheckArguments(instruction, context, 2))
            {
                return false;
            }

            uint offset = *(uint*)instruction.Arguments[0];
            uint count = *(uint*)instruction.Arguments[1];

            Pop(offset, count, context);
            return true;
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, bool> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream)
        {
            stream.Read<uint>();
            stream.Read<uint>();
            return 2;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = stream.ReadSafePointer<uint>();
            arguments[1] = stream.ReadSafePointer<uint>();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.PopOffsetRepeat"/> operation executor
        /// </summary>
        public static readonly DSharpPopOffsetRepeatInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Pop(uint offset, uint count, DSharpExecutionContext context)
        {
            for (int i = 0; i < count; i++)
            {
                context.Stack.Pop(offset);
            }
        }

        private static bool InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

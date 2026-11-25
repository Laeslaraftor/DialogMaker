using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using DialogMaker.Core.Scripting.Runtime.Numbers.MathExecutors;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.BitInvert"/> operation
    /// </summary>
    public unsafe class DSharpBitInvertInstructionExecutor : DSharpInstructionExecutor
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
            return 0;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.BitInvert"/> operation executor
        /// </summary>
        public static readonly DSharpBitInvertInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (CheckStackValues(instruction, ref context, 1))
            {
                return;
            }
#endif

            var lastValue = context.Stack.Peek();
            var obj = lastValue.ReadAsStructure();
            var number = DSharpNumber.FromObject(obj);

            number = DSharpNumberMathExecutor.BitInvert(ref number);
            Unsafe.CopyBlockUnaligned(obj + 1, &number, (uint)number.Size);
        }

        #endregion
    }
}

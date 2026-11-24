using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using DialogMaker.Core.Scripting.Runtime.Numbers.MathExecutors;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Increment"/> operation
    /// </summary>
    public unsafe class DSharpIncrementInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public unsafe override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (CheckStackValues(instruction, context, 1, out var error))
            {
                return error;
            }
#endif

            var lastValue = context.Stack.Peek();
            var number = lastValue.ReadAsNumber();
            var obj = lastValue.ReadAsObject();

            number = DSharpNumberMathExecutor.Increment(ref number);
            Unsafe.CopyBlockUnaligned(DSharpObject.GetData(obj), &number, (uint)number.Size);
            //number.Write(DSharpObject.GetData(obj));

            return DSharpMethodExecutionCallback.Complete();
        }

        public override unsafe delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public unsafe override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            return 0;
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Increment"/> operation executor
        /// </summary>
        public static readonly DSharpIncrementInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

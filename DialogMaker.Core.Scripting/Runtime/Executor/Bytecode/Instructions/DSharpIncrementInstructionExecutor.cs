using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Increment"/> operation
    /// </summary>
    public class DSharpIncrementInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public unsafe override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (CheckStackValues(instruction, context, 1, out var error))
            {
                return error;
            }

            var lastValue = context.Stack.Peek();
            var decimalValue = lastValue.ReadAsDecimal();

            if (decimalValue == null)
            {
                return context.ThrowExecutionException($"Increment operation requires number at last value in stack, got: \"{lastValue.ValueType}\"");
            }

            decimal number = decimalValue.Value + 1;
            var obj = (DSharpObject*)lastValue.StackPointer;

            context.ObjectsContainer.WriteNumber(obj, number);

            return DSharpMethodExecutionCallback.Complete();
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
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

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.JumpIndexed"/> operation
    /// </summary>
    public class DSharpJumpIndexedInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (CheckStackValues(instruction, context, 1, out var error))
            {
                return error;
            }

            var value = context.Stack.Peek();

            if (!value.IsNumber)
            {
                return context.ThrowExecutionException($"Jumping by index that placed in stack requires number value, got: {value.ValueType}");
            }

            var decimalNumber = value.ReadAsDecimal().GetValueOrDefault();
            uint index;

            if (uint.MinValue > decimalNumber)
            {
                index = 0;
            }
            else if (decimalNumber > uint.MaxValue)
            {
                index = uint.MaxValue;
            }
            else
            {
                index = (uint)decimalNumber;
            }

            context.InstructionIndex = index;

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
        /// Global instance of <see cref="DSharpBytecodeOperation.JumpIndexed"/> operation executor
        /// </summary>
        public static readonly DSharpJumpIndexedInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

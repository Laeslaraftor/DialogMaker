using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.JumpIndexed"/> operation
    /// </summary>
    public class DSharpJumpIndexedInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override bool Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (!CheckStackValues(instruction, context, 1))
            {
                return false;
            }

            var value = context.Stack.Peek();

            if (!value.IsNumber)
            {
                context.ThrowExecutionException($"Jumping by index that placed in stack requires number value, got: {value.ValueType}");
                return false;
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

            return true;
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, bool> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream)
        {
            return 0;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream, UnmanagedArray<nint> arguments)
        {
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.JumpIndexed"/> operation executor
        /// </summary>
        public static readonly DSharpJumpIndexedInstructionExecutor Instance = new();

        private static bool InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

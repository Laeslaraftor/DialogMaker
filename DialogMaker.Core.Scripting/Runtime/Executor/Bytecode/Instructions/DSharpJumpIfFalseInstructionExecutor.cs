namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.JumpIfFalse"/> operation
    /// </summary>
    public class DSharpJumpIfFalseInstructionExecutor : DSharpBaseJumpInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, bool> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override bool CanJump(DSharpRuntimeInstruction instruction, DSharpExecutionContext context)
        {
            var lastValue = context.Stack.Peek();
            return !lastValue.Read<bool>();
        }
        protected override bool Validate(DSharpRuntimeInstruction instruction, DSharpExecutionContext context)
        {
            if (base.Validate(instruction, context) && CheckStackValues(instruction, context, 1))
            {
                var lastValue = context.Stack.Peek();

                if (lastValue.ValueType != DSharpStackValueType.Bool)
                {
                    context.ThrowExecutionException($"Unable to jump with condition: last stack value should be boolean, got {lastValue.ValueType}");
                    return false;
                }

                return true;
            }

            return false;
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.JumpIfFalse"/> operation executor
        /// </summary>
        public static readonly DSharpJumpIfFalseInstructionExecutor Instance = new();

        private static bool InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

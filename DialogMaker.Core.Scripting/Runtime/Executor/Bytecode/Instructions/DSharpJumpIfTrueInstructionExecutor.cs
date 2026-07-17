using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.JumpIfTrue"/> operation
    /// </summary>
    public class DSharpJumpIfTrueInstructionExecutor : DSharpBaseJumpInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override bool CanJump(DSharpRuntimeInstruction instruction, DSharpExecutionContext context)
        {
            var lastValue = context.Stack.Peek();
            return lastValue.Read<bool>();
        }
        protected override bool IsNotValid(DSharpRuntimeInstruction instruction, DSharpExecutionContext context, [NotNullWhen(true)] out DSharpMethodExecutionCallback errorCallback)
        {
            if (base.IsNotValid(instruction, context, out errorCallback))
            {
                return true;
            }
            if (!CheckStackValues(instruction, context, 1, out errorCallback))
            {
                var lastValue = context.Stack.Peek();

                if (lastValue.ValueType != DSharpStackValueType.Bool)
                {
                    errorCallback = context.ThrowExecutionException($"Unable to jump with condition: last stack value should be boolean, got {lastValue.ValueType}");
                    return true;
                }

                return false;
            }

            return true;
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.JumpIfTrue"/> operation executor
        /// </summary>
        public static readonly DSharpJumpIfTrueInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

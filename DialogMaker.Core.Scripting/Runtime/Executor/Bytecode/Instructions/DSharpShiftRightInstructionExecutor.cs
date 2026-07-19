using System.Numerics;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.ShiftRight"/> operation
    /// </summary>
    public class DSharpShiftRightInstructionExecutor : DSharpBitMathInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        protected override bool PerformMathOperation(bool left, bool right)
        {
            return false;
        }
        protected override BigInteger PerformMathOperation(BigInteger left, BigInteger right)
        {
            int shift;

            if (right > int.MaxValue)
            {
                shift = int.MaxValue;
            }
            else if (right < int.MinValue)
            {
                shift = int.MinValue;
            }
            else
            {
                shift = (int)right;
            }

            return left >> shift;
        }

        protected override unsafe bool CanPerform(DSharpStack.FrameInfo left, DSharpStack.FrameInfo right, DSharpExecutionContext context)
        {
            return left.ObjectType != context.TypesProvider.Boolean && left.ObjectType != context.TypesProvider.Boolean;
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.ShiftRight"/> operation executor
        /// </summary>
        public static readonly DSharpShiftRightInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

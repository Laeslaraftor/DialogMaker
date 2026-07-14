using System.Numerics;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.ShiftLeft"/> operation
    /// </summary>
    public class DSharpShiftLeftInstructionExecutor : DSharpBitMathInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, bool> GetExecutorPointer()
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

            return left << shift;
        }

        protected override bool CanPerform(DSharpStackValueType left, DSharpStackValueType right)
        {
            return left != DSharpStackValueType.Bool && right != DSharpStackValueType.Bool;
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.ShiftLeft"/> operation executor
        /// </summary>
        public static readonly DSharpShiftLeftInstructionExecutor Instance = new();

        private static bool InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

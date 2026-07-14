using System.Numerics;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.And"/> operation
    /// </summary>
    public class DSharpAndInstructionExecutor : DSharpBitMathInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, bool> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        protected override bool PerformMathOperation(bool left, bool right)
        {
            return left & right;
        }
        protected override BigInteger PerformMathOperation(BigInteger left, BigInteger right)
        {
            return left & right;
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.And"/> operation executor
        /// </summary>
        public static readonly DSharpAndInstructionExecutor Instance = new();

        private static bool InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}

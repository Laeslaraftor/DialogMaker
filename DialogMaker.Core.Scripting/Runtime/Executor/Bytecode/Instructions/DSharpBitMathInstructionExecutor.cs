using System.Numerics;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base class of bit math instruction executor
    /// </summary>
    public abstract class DSharpBitMathInstructionExecutor : DSharpMathInstructionExecutor
    {
        protected override decimal PerformMathOperation(decimal left, decimal right)
        {
            var leftBigInt = ToBigInteger(left);
            var rightBigInt = ToBigInteger(right);

            return (decimal)PerformMathOperation(leftBigInt, rightBigInt);
        }
        protected override bool CanPerform(DSharpStack.FrameInfo left, DSharpStack.FrameInfo right, DSharpExecutionContext context)
        {
            return true;
        }

        /// <summary>
        /// Perform math operation between <see cref="BigInteger"/> values
        /// </summary>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        /// <returns>Result of math operation</returns>
        protected abstract BigInteger PerformMathOperation(BigInteger left, BigInteger right);

        private BigInteger ToBigInteger(decimal value)
        {
            if (value > ulong.MaxValue)
            {
                return new(decimal.ToUInt64(value));
            }

            return new(decimal.ToInt64(value));
        }
    }
}

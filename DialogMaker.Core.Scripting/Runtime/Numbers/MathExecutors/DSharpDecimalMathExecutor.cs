using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Numbers.MathExecutors
{
    internal sealed class DSharpDecimalMathExecutor : IDSharpNumberMathExecutor
    {
        public static readonly DSharpDecimalMathExecutor Instance = new();

        public DSharpNumber Add(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<decimal>() + right.Read<decimal>());
        }
        public DSharpNumber Subtract(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<decimal>() - right.Read<decimal>());
        }
        public DSharpNumber Mod(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<decimal>() % right.Read<decimal>());
        }
        public DSharpNumber Multiply(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<decimal>() * right.Read<decimal>());
        }
        public DSharpNumber Divide(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<decimal>() / right.Read<decimal>());
        }
        public DSharpNumber Xor(ref DSharpNumber left, ref DSharpNumber right)
        {
            return (decimal)unchecked((long)left.Read<decimal>() ^ (long)right.Read<decimal>());
        }
        public DSharpNumber Or(ref DSharpNumber left, ref DSharpNumber right)
        {
            return (decimal)((long)left.Read<decimal>() | (long)right.Read<decimal>());
        }
        public DSharpNumber And(ref DSharpNumber left, ref DSharpNumber right)
        {
            return (decimal)((long)left.Read<decimal>() & (long)right.Read<decimal>());
        }
        public DSharpNumber ShiftLeft(ref DSharpNumber left, ref DSharpNumber right)
        {
            return (decimal)unchecked((long)left.Read<decimal>() << (int)right.Read<decimal>());
        }
        public DSharpNumber ShiftRight(ref DSharpNumber left, ref DSharpNumber right)
        {
            return (decimal)unchecked((long)left.Read<decimal>() >> (int)right.Read<decimal>());
        }

        public DSharpNumber LogicalAnd(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<decimal>() != 0 && right.Read<decimal>() != 0;
        }
        public DSharpNumber LogicalOr(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<decimal>() != 0 || right.Read<decimal>() != 0;
        }

        public DSharpNumber LogicalEquals(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<decimal>() == right.Read<decimal>();
        }
        public DSharpNumber LogicalNotEquals(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<decimal>() != right.Read<decimal>();
        }

        public DSharpNumber LogicalLess(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<decimal>() < right.Read<decimal>();
        }
        public DSharpNumber LogicalLessOrEquals(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<decimal>() <= right.Read<decimal>();
        }
        public DSharpNumber LogicalGreater(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<decimal>() > right.Read<decimal>();
        }
        public DSharpNumber LogicalGreaterOrEquals(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<decimal>() >= right.Read<decimal>();
        }

        public DSharpNumber Invert(ref DSharpNumber value)
        {
            return -value.Read<decimal>();
        }
        public DSharpNumber Not(ref DSharpNumber value)
        {
            var number = value.Read<decimal>();
            return number == 0 ? decimal.One : decimal.Zero;
        }
        public DSharpNumber Increment(ref DSharpNumber value)
        {
            return unchecked(value.Read<decimal>() + 1);
        }
        public DSharpNumber Decrement(ref DSharpNumber value)
        {
            return unchecked(value.Read<decimal>() - 1);
        }
        public unsafe DSharpNumber BitInvert(ref DSharpNumber value)
        {
            var number = value.Read<decimal>();
            var bytes = (uint*)&number;

            bytes[0] = ~bytes[0];
            bytes[1] = ~bytes[1];
            bytes[2] = ~bytes[2];

            return number;
        }
    }
}

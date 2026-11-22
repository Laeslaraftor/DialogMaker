using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Numbers.MathExecutors
{
    internal sealed class DSharpDecimalMathExecutor : IDSharpNumberMathExecutor
    {
        public static readonly DSharpDecimalMathExecutor Instance = new();

        public DSharpNumber Add(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<decimal>() + right.Read<decimal>());
        }
        public DSharpNumber Subtract(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<decimal>() - right.Read<decimal>());
        }
        public DSharpNumber Mod(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<decimal>() % right.Read<decimal>());
        }
        public DSharpNumber Multiply(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<decimal>() * right.Read<decimal>());
        }
        public DSharpNumber Divide(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<decimal>() / right.Read<decimal>());
        }
        public DSharpNumber Xor(DSharpNumber left, DSharpNumber right)
        {
            return (decimal)unchecked((long)left.Read<decimal>() ^ (long)right.Read<decimal>());
        }
        public DSharpNumber Or(DSharpNumber left, DSharpNumber right)
        {
            return (decimal)((long)left.Read<decimal>() | (long)right.Read<decimal>());
        }
        public DSharpNumber And(DSharpNumber left, DSharpNumber right)
        {
            return (decimal)((long)left.Read<decimal>() & (long)right.Read<decimal>());
        }
        public DSharpNumber ShiftLeft(DSharpNumber left, DSharpNumber right)
        {
            return (decimal)unchecked((long)left.Read<decimal>() << (int)right.Read<decimal>());
        }
        public DSharpNumber ShiftRight(DSharpNumber left, DSharpNumber right)
        {
            return (decimal)unchecked((long)left.Read<decimal>() >> (int)right.Read<decimal>());
        }

        public DSharpNumber LogicalAnd(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<decimal>() != 0 && right.Read<decimal>() != 0;
        }
        public DSharpNumber LogicalOr(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<decimal>() != 0 || right.Read<decimal>() != 0;
        }

        public DSharpNumber LogicalEquals(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<decimal>() == right.Read<decimal>();
        }
        public DSharpNumber LogicalNotEquals(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<decimal>() != right.Read<decimal>();
        }

        public DSharpNumber LogicalLess(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<decimal>() < right.Read<decimal>();
        }
        public DSharpNumber LogicalLessOrEquals(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<decimal>() <= right.Read<decimal>();
        }
        public DSharpNumber LogicalGreater(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<decimal>() > right.Read<decimal>();
        }
        public DSharpNumber LogicalGreaterOrEquals(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<decimal>() >= right.Read<decimal>();
        }

        public DSharpNumber Invert(DSharpNumber value)
        {
            return -value.Read<decimal>();
        }
        public DSharpNumber Not(DSharpNumber value)
        {
            var number = value.Read<decimal>();
            return number == 0 ? decimal.One : decimal.Zero;
        }
        public DSharpNumber Increment(DSharpNumber value)
        {
            return unchecked(value.Read<decimal>() + 1);
        }
        public DSharpNumber Decrement(DSharpNumber value)
        {
            return unchecked(value.Read<decimal>() - 1);
        }
        public unsafe DSharpNumber BitInvert(DSharpNumber value)
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

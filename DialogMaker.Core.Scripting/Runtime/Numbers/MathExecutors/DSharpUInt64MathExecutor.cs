namespace DialogMaker.Core.Scripting.Runtime.Numbers.MathExecutors
{
    internal sealed class DSharpUInt64MathExecutor : IDSharpNumberMathExecutor
    {
        public static readonly DSharpUInt64MathExecutor Instance = new();

        public DSharpNumber Add(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<ulong>() + right.Read<ulong>());
        }
        public DSharpNumber Subtract(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<ulong>() - right.Read<ulong>());
        }
        public DSharpNumber Mod(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<ulong>() % right.Read<ulong>());
        }
        public DSharpNumber Multiply(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<ulong>() * right.Read<ulong>());
        }
        public DSharpNumber Divide(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<ulong>() / right.Read<ulong>());
        }
        public DSharpNumber Xor(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<ulong>() ^ right.Read<ulong>());
        }
        public DSharpNumber Or(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<ulong>() | right.Read<ulong>();
        }
        public DSharpNumber And(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<ulong>() & right.Read<ulong>();
        }
        public DSharpNumber ShiftLeft(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<ulong>() << (int)right.Read<ulong>());
        }
        public DSharpNumber ShiftRight(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<ulong>() >> (int)right.Read<ulong>());
        }

        public DSharpNumber LogicalAnd(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<ulong>() != 0 && right.Read<ulong>() != 0;
        }
        public DSharpNumber LogicalOr(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<ulong>() != 0 || right.Read<ulong>() != 0;
        }

        public DSharpNumber LogicalEquals(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<ulong>() == right.Read<ulong>();
        }
        public DSharpNumber LogicalNotEquals(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<ulong>() != right.Read<ulong>();
        }

        public DSharpNumber LogicalLess(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<ulong>() < right.Read<ulong>();
        }
        public DSharpNumber LogicalLessOrEquals(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<ulong>() <= right.Read<ulong>();
        }
        public DSharpNumber LogicalGreater(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<ulong>() > right.Read<ulong>();
        }
        public DSharpNumber LogicalGreaterOrEquals(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<ulong>() >= right.Read<ulong>();
        }

        public DSharpNumber Invert(DSharpNumber value)
        {
            var number = value.Read<ulong>();
            return number == 0 ? ulong.MaxValue : 0;
        }
        public DSharpNumber Not(DSharpNumber value)
        {
            var number = value.Read<ulong>();
            return number == 0 ? 1UL : 0UL;
        }
        public DSharpNumber Increment(DSharpNumber value)
        {
            return unchecked(value.Read<ulong>() + 1);
        }
        public DSharpNumber Decrement(DSharpNumber value)
        {
            return unchecked(value.Read<ulong>() - 1);
        }
        public DSharpNumber BitInvert(DSharpNumber value)
        {
            return ~value.Read<ulong>();
        }
    }
}

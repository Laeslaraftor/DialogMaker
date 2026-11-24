namespace DialogMaker.Core.Scripting.Runtime.Numbers.MathExecutors
{
    internal sealed class DSharpUInt64MathExecutor : IDSharpNumberMathExecutor
    {
        public static readonly DSharpUInt64MathExecutor Instance = new();

        public DSharpNumber Add(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<ulong>() + right.Read<ulong>());
        }
        public DSharpNumber Subtract(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<ulong>() - right.Read<ulong>());
        }
        public DSharpNumber Mod(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<ulong>() % right.Read<ulong>());
        }
        public DSharpNumber Multiply(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<ulong>() * right.Read<ulong>());
        }
        public DSharpNumber Divide(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<ulong>() / right.Read<ulong>());
        }
        public DSharpNumber Xor(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<ulong>() ^ right.Read<ulong>());
        }
        public DSharpNumber Or(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<ulong>() | right.Read<ulong>();
        }
        public DSharpNumber And(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<ulong>() & right.Read<ulong>();
        }
        public DSharpNumber ShiftLeft(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<ulong>() << (int)right.Read<ulong>());
        }
        public DSharpNumber ShiftRight(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<ulong>() >> (int)right.Read<ulong>());
        }

        public DSharpNumber LogicalAnd(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<ulong>() != 0 && right.Read<ulong>() != 0;
        }
        public DSharpNumber LogicalOr(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<ulong>() != 0 || right.Read<ulong>() != 0;
        }

        public DSharpNumber LogicalEquals(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<ulong>() == right.Read<ulong>();
        }
        public DSharpNumber LogicalNotEquals(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<ulong>() != right.Read<ulong>();
        }

        public DSharpNumber LogicalLess(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<ulong>() < right.Read<ulong>();
        }
        public DSharpNumber LogicalLessOrEquals(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<ulong>() <= right.Read<ulong>();
        }
        public DSharpNumber LogicalGreater(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<ulong>() > right.Read<ulong>();
        }
        public DSharpNumber LogicalGreaterOrEquals(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<ulong>() >= right.Read<ulong>();
        }

        public DSharpNumber Invert(ref DSharpNumber value)
        {
            var number = value.Read<ulong>();
            return number == 0 ? ulong.MaxValue : 0;
        }
        public DSharpNumber Not(ref DSharpNumber value)
        {
            var number = value.Read<ulong>();
            return number == 0 ? 1UL : 0UL;
        }
        public DSharpNumber Increment(ref DSharpNumber value)
        {
            return unchecked(value.Read<ulong>() + 1);
        }
        public DSharpNumber Decrement(ref DSharpNumber value)
        {
            return unchecked(value.Read<ulong>() - 1);
        }
        public DSharpNumber BitInvert(ref DSharpNumber value)
        {
            return ~value.Read<ulong>();
        }
    }
}

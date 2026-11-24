namespace DialogMaker.Core.Scripting.Runtime.Numbers.MathExecutors
{
    internal sealed class DSharpInt64MathExecutor : IDSharpNumberMathExecutor
    {
        public static readonly DSharpInt64MathExecutor Instance = new();

        public DSharpNumber Add(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<long>() + right.Read<long>());
        }
        public DSharpNumber Subtract(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<long>() - right.Read<long>());
        }
        public DSharpNumber Mod(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<long>() % right.Read<long>());
        }
        public DSharpNumber Multiply(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<long>() * right.Read<long>());
        }
        public DSharpNumber Divide(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<long>() / right.Read<long>());
        }
        public DSharpNumber Xor(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<long>() ^ right.Read<long>());
        }
        public DSharpNumber Or(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<long>() | right.Read<long>();
        }
        public DSharpNumber And(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<long>() & right.Read<long>();
        }
        public DSharpNumber ShiftLeft(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<long>() << (int)right.Read<long>());
        }
        public DSharpNumber ShiftRight(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<long>() >> (int)right.Read<long>());
        }

        public DSharpNumber LogicalAnd(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<long>() != 0 && right.Read<long>() != 0;
        }
        public DSharpNumber LogicalOr(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<long>() != 0 || right.Read<long>() != 0;
        }

        public DSharpNumber LogicalEquals(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<long>() == right.Read<long>();
        }
        public DSharpNumber LogicalNotEquals(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<long>() != right.Read<long>();
        }

        public DSharpNumber LogicalLess(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<long>() < right.Read<long>();
        }
        public DSharpNumber LogicalLessOrEquals(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<long>() <= right.Read<long>();
        }
        public DSharpNumber LogicalGreater(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<long>() > right.Read<long>();
        }
        public DSharpNumber LogicalGreaterOrEquals(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<long>() >= right.Read<long>();
        }

        public DSharpNumber Invert(ref DSharpNumber value)
        {
            return -value.Read<long>();
        }
        public DSharpNumber Not(ref DSharpNumber value)
        {
            var number = value.Read<long>();
            return number == 0 ? 1L : 0L;
        }
        public DSharpNumber Increment(ref DSharpNumber value)
        {
            return unchecked(value.Read<long>() + 1);
        }
        public DSharpNumber Decrement(ref DSharpNumber value)
        {
            return unchecked(value.Read<long>() - 1);
        }
        public DSharpNumber BitInvert(ref DSharpNumber value)
        {
            return ~value.Read<long>();
        }
    }
}

namespace DialogMaker.Core.Scripting.Runtime.Numbers.MathExecutors
{
    internal sealed class DSharpInt64MathExecutor : IDSharpNumberMathExecutor
    {
        public static readonly DSharpInt64MathExecutor Instance = new();

        public DSharpNumber Add(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<long>() + right.Read<long>());
        }
        public DSharpNumber Subtract(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<long>() - right.Read<long>());
        }
        public DSharpNumber Mod(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<long>() % right.Read<long>());
        }
        public DSharpNumber Multiply(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<long>() * right.Read<long>());
        }
        public DSharpNumber Divide(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<long>() / right.Read<long>());
        }
        public DSharpNumber Xor(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<long>() ^ right.Read<long>());
        }
        public DSharpNumber Or(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<long>() | right.Read<long>();
        }
        public DSharpNumber And(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<long>() & right.Read<long>();
        }
        public DSharpNumber ShiftLeft(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<long>() << (int)right.Read<long>());
        }
        public DSharpNumber ShiftRight(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<long>() >> (int)right.Read<long>());
        }

        public DSharpNumber LogicalAnd(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<long>() != 0 && right.Read<long>() != 0;
        }
        public DSharpNumber LogicalOr(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<long>() != 0 || right.Read<long>() != 0;
        }

        public DSharpNumber LogicalEquals(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<long>() == right.Read<long>();
        }
        public DSharpNumber LogicalNotEquals(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<long>() != right.Read<long>();
        }

        public DSharpNumber LogicalLess(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<long>() < right.Read<long>();
        }
        public DSharpNumber LogicalLessOrEquals(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<long>() <= right.Read<long>();
        }
        public DSharpNumber LogicalGreater(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<long>() > right.Read<long>();
        }
        public DSharpNumber LogicalGreaterOrEquals(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<long>() >= right.Read<long>();
        }

        public DSharpNumber Invert(DSharpNumber value)
        {
            return -value.Read<long>();
        }
        public DSharpNumber Not(DSharpNumber value)
        {
            var number = value.Read<long>();
            return number == 0 ? 1L : 0L;
        }
        public DSharpNumber Increment(DSharpNumber value)
        {
            return unchecked(value.Read<long>() + 1);
        }
        public DSharpNumber Decrement(DSharpNumber value)
        {
            return unchecked(value.Read<long>() - 1);
        }
        public DSharpNumber BitInvert(DSharpNumber value)
        {
            return ~value.Read<long>();
        }
    }
}

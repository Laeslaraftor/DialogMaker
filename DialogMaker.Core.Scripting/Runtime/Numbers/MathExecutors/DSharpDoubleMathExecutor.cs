using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Numbers.MathExecutors
{
    internal sealed class DSharpDoubleMathExecutor : IDSharpNumberMathExecutor
    {
        public static readonly DSharpDoubleMathExecutor Instance = new();

        public DSharpNumber Add(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<double>() + right.Read<double>());
        }
        public DSharpNumber Subtract(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<double>() - right.Read<double>());
        }
        public DSharpNumber Mod(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<double>() % right.Read<double>());
        }
        public DSharpNumber Multiply(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<double>() * right.Read<double>());
        }
        public DSharpNumber Divide(DSharpNumber left, DSharpNumber right)
        {
            return unchecked(left.Read<double>() / right.Read<double>());
        }
        public DSharpNumber Xor(DSharpNumber left, DSharpNumber right)
        {
            return (double)unchecked((long)left.Read<double>() ^ (long)right.Read<double>());
        }
        public DSharpNumber Or(DSharpNumber left, DSharpNumber right)
        {
            return (double)((long)left.Read<double>() | (long)right.Read<double>());
        }
        public DSharpNumber And(DSharpNumber left, DSharpNumber right)
        {
            return (double)((long)left.Read<double>() & (long)right.Read<double>());
        }
        public DSharpNumber ShiftLeft(DSharpNumber left, DSharpNumber right)
        {
            return (double)unchecked((long)left.Read<double>() << (int)right.Read<double>());
        }
        public DSharpNumber ShiftRight(DSharpNumber left, DSharpNumber right)
        {
            return (double)unchecked((long)left.Read<double>() >> (int)right.Read<double>());
        }

        public DSharpNumber LogicalAnd(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<double>() != 0 && right.Read<double>() != 0;
        }
        public DSharpNumber LogicalOr(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<double>() != 0 || right.Read<double>() != 0;
        }

        public DSharpNumber LogicalEquals(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<double>() == right.Read<double>();
        }
        public DSharpNumber LogicalNotEquals(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<double>() != right.Read<double>();
        }

        public DSharpNumber LogicalLess(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<double>() < right.Read<double>();
        }
        public DSharpNumber LogicalLessOrEquals(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<double>() <= right.Read<double>();
        }
        public DSharpNumber LogicalGreater(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<double>() > right.Read<double>();
        }
        public DSharpNumber LogicalGreaterOrEquals(DSharpNumber left, DSharpNumber right)
        {
            return left.Read<double>() >= right.Read<double>();
        }

        public DSharpNumber Invert(DSharpNumber value)
        {
            return -value.Read<double>();
        }
        public DSharpNumber Not(DSharpNumber value)
        {
            var number = value.Read<double>();
            return number == 0 ? 1d : 0d;
        }
        public DSharpNumber Increment(DSharpNumber value)
        {
            return unchecked(value.Read<double>() + 1);
        }
        public DSharpNumber Decrement(DSharpNumber value)
        {
            return unchecked(value.Read<double>() - 1);
        }
        public DSharpNumber BitInvert(DSharpNumber value)
        {
            var number = value.Read<double>();
            ref long bits = ref Unsafe.As<double, long>(ref number);
            bits = ~bits;

            return number;
        }
    }
}

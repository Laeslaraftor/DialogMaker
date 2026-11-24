using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Numbers.MathExecutors
{
    internal sealed class DSharpDoubleMathExecutor : IDSharpNumberMathExecutor
    {
        public static readonly DSharpDoubleMathExecutor Instance = new();

        public DSharpNumber Add(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<double>() + right.Read<double>());
        }
        public DSharpNumber Subtract(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<double>() - right.Read<double>());
        }
        public DSharpNumber Mod(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<double>() % right.Read<double>());
        }
        public DSharpNumber Multiply(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<double>() * right.Read<double>());
        }
        public DSharpNumber Divide(ref DSharpNumber left, ref DSharpNumber right)
        {
            return unchecked(left.Read<double>() / right.Read<double>());
        }
        public DSharpNumber Xor(ref DSharpNumber left, ref DSharpNumber right)
        {
            return (double)unchecked((long)left.Read<double>() ^ (long)right.Read<double>());
        }
        public DSharpNumber Or(ref DSharpNumber left, ref DSharpNumber right)
        {
            return (double)((long)left.Read<double>() | (long)right.Read<double>());
        }
        public DSharpNumber And(ref DSharpNumber left, ref DSharpNumber right)
        {
            return (double)((long)left.Read<double>() & (long)right.Read<double>());
        }
        public DSharpNumber ShiftLeft(ref DSharpNumber left, ref DSharpNumber right)
        {
            return (double)unchecked((long)left.Read<double>() << (int)right.Read<double>());
        }
        public DSharpNumber ShiftRight(ref DSharpNumber left, ref DSharpNumber right)
        {
            return (double)unchecked((long)left.Read<double>() >> (int)right.Read<double>());
        }

        public DSharpNumber LogicalAnd(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<double>() != 0 && right.Read<double>() != 0;
        }
        public DSharpNumber LogicalOr(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<double>() != 0 || right.Read<double>() != 0;
        }

        public DSharpNumber LogicalEquals(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<double>() == right.Read<double>();
        }
        public DSharpNumber LogicalNotEquals(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<double>() != right.Read<double>();
        }

        public DSharpNumber LogicalLess(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<double>() < right.Read<double>();
        }
        public DSharpNumber LogicalLessOrEquals(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<double>() <= right.Read<double>();
        }
        public DSharpNumber LogicalGreater(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<double>() > right.Read<double>();
        }
        public DSharpNumber LogicalGreaterOrEquals(ref DSharpNumber left, ref DSharpNumber right)
        {
            return left.Read<double>() >= right.Read<double>();
        }

        public DSharpNumber Invert(ref DSharpNumber value)
        {
            return -value.Read<double>();
        }
        public DSharpNumber Not(ref DSharpNumber value)
        {
            var number = value.Read<double>();
            return number == 0 ? 1d : 0d;
        }
        public DSharpNumber Increment(ref DSharpNumber value)
        {
            return unchecked(value.Read<double>() + 1);
        }
        public DSharpNumber Decrement(ref DSharpNumber value)
        {
            return unchecked(value.Read<double>() - 1);
        }
        public DSharpNumber BitInvert(ref DSharpNumber value)
        {
            var number = value.Read<double>();
            ref long bits = ref Unsafe.As<double, long>(ref number);
            bits = ~bits;

            return number;
        }
    }
}

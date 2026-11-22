using DialogMaker.Core.Scripting.Compiler.Ast;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Numbers.MathExecutors
{
    internal sealed class DSharpNumberMathExecutor : IDSharpNumberMathExecutor
    {
        #region Operators

        public DSharpNumber Add(DSharpNumber left, DSharpNumber right)
        {
            var kind = GetKindAndBigger(left, right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[kind].Add(kindLeft, kindRight).Convert(bigger);
        }
        public DSharpNumber Subtract(DSharpNumber left, DSharpNumber right)
        {
            var kind = GetKindAndBigger(left, right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[kind].Subtract(kindLeft, kindRight).Convert(bigger);
        }
        public DSharpNumber Mod(DSharpNumber left, DSharpNumber right)
        {
            var kind = GetKindAndBigger(left, right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[kind].Mod(kindLeft, kindRight).Convert(bigger);
        }
        public DSharpNumber Multiply(DSharpNumber left, DSharpNumber right)
        {
            var kind = GetKindAndBigger(left, right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[kind].Multiply(kindLeft, kindRight).Convert(bigger);
        }
        public DSharpNumber Divide(DSharpNumber left, DSharpNumber right)
        {
            var kind = GetKindAndBigger(left, right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[kind].Divide(kindLeft, kindRight).Convert(bigger);
        }
        public DSharpNumber Xor(DSharpNumber left, DSharpNumber right)
        {
            var kind = GetKindAndBigger(left, right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[kind].Xor(kindLeft, kindRight).Convert(bigger);
        }
        public DSharpNumber Or(DSharpNumber left, DSharpNumber right)
        {
            var kind = GetKindAndBigger(left, right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[kind].Or(kindLeft, kindRight).Convert(bigger);
        }
        public DSharpNumber And(DSharpNumber left, DSharpNumber right)
        {
            var kind = GetKindAndBigger(left, right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[kind].And(kindLeft, kindRight).Convert(bigger);
        }
        public DSharpNumber ShiftLeft(DSharpNumber left, DSharpNumber right)
        {
            var kind = GetKindAndBigger(left, right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[kind].ShiftLeft(kindLeft, kindRight).Convert(bigger);
        }
        public DSharpNumber ShiftRight(DSharpNumber left, DSharpNumber right)
        {
            var kind = GetKindAndBigger(left, right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[kind].ShiftRight(kindLeft, kindRight).Convert(bigger);
        }

        public DSharpNumber LogicalAnd(DSharpNumber left, DSharpNumber right)
        {
            var kind = GetKind(left, right, out var kindLeft, out var kindRight);
            return _mathExecutors[kind].LogicalAnd(kindLeft, kindRight);
        }
        public DSharpNumber LogicalOr(DSharpNumber left, DSharpNumber right)
        {
            var kind = GetKind(left, right, out var kindLeft, out var kindRight);
            return _mathExecutors[kind].LogicalOr(kindLeft, kindRight);
        }

        public DSharpNumber LogicalEquals(DSharpNumber left, DSharpNumber right)
        {
            var kind = GetKind(left, right, out var kindLeft, out var kindRight);
            return _mathExecutors[kind].LogicalEquals(kindLeft, kindRight);
        }
        public DSharpNumber LogicalNotEquals(DSharpNumber left, DSharpNumber right)
        {
            var kind = GetKind(left, right, out var kindLeft, out var kindRight);
            return _mathExecutors[kind].LogicalNotEquals(kindLeft, kindRight);
        }

        public DSharpNumber LogicalLess(DSharpNumber left, DSharpNumber right)
        {
            var kind = GetKind(left, right, out var kindLeft, out var kindRight);
            return _mathExecutors[kind].LogicalLess(kindLeft, kindRight);
        }
        public DSharpNumber LogicalLessOrEquals(DSharpNumber left, DSharpNumber right)
        {
            var kind = GetKind(left, right, out var kindLeft, out var kindRight);
            return _mathExecutors[kind].LogicalLessOrEquals(kindLeft, kindRight);
        }
        public DSharpNumber LogicalGreater(DSharpNumber left, DSharpNumber right)
        {
            var kind = GetKind(left, right, out var kindLeft, out var kindRight);
            return _mathExecutors[kind].LogicalGreater(kindLeft, kindRight);
        }
        public DSharpNumber LogicalGreaterOrEquals(DSharpNumber left, DSharpNumber right)
        {
            var kind = GetKind(left, right, out var kindLeft, out var kindRight);
            return _mathExecutors[kind].LogicalGreaterOrEquals(kindLeft, kindRight);
        }

        public DSharpNumber Invert(DSharpNumber value)
        {
            var kind = GetKindAndConvert(value, out var kindValue);
            return _mathExecutors[kind].Invert(kindValue).Convert(value.Type);
        }
        public DSharpNumber Not(DSharpNumber value)
        {
            var kind = GetKindAndConvert(value, out var kindValue);
            return _mathExecutors[kind].Not(kindValue).Convert(value.Type);
        }
        public DSharpNumber Increment(DSharpNumber value)
        {
            var kind = GetKindAndConvert(value, out var kindValue);
            return _mathExecutors[kind].Increment(kindValue).Convert(value.Type);
        }
        public DSharpNumber Decrement(DSharpNumber value)
        {
            var kind = GetKindAndConvert(value, out var kindValue);
            return _mathExecutors[kind].Decrement(kindValue).Convert(value.Type);
        }
        public DSharpNumber BitInvert(DSharpNumber value)
        {
            var kind = GetKindAndConvert(value, out var kindValue);
            return _mathExecutors[kind].BitInvert(kindValue).Convert(value.Type);
        }

        #endregion

        #region Static

        public static readonly DSharpNumberMathExecutor Instance = new();

        private static readonly Dictionary<DSharpNumberKind, IDSharpNumberMathExecutor> _mathExecutors = new()
        {
            [DSharpNumberKind.Int64] = DSharpInt64MathExecutor.Instance,
            [DSharpNumberKind.UInt64] = DSharpUInt64MathExecutor.Instance,
            [DSharpNumberKind.Double] = DSharpDoubleMathExecutor.Instance,
            [DSharpNumberKind.Decimal] = DSharpDecimalMathExecutor.Instance,
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber Perform(DSharpNumber value, DSharpUnaryOperator @operator)
        {
            return @operator switch
            {
                DSharpUnaryOperator.Minus => Instance.Invert(value),
                DSharpUnaryOperator.Not => Instance.Not(value),
                DSharpUnaryOperator.Increment => Instance.Increment(value),
                DSharpUnaryOperator.Decrement => Instance.Decrement(value),
                DSharpUnaryOperator.BitInvert => Instance.BitInvert(value),
                _ => throw new ArgumentException($"Invalid unary operator \"{@operator}\"", nameof(@operator))
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber Perform(DSharpNumber left, DSharpNumber right, DSharpBinaryOperator @operator)
        {
            return @operator switch
            {
                DSharpBinaryOperator.Plus => Instance.Add(left, right),
                DSharpBinaryOperator.Minus => Instance.Subtract(left, right),
                DSharpBinaryOperator.Multiply => Instance.Multiply(left, right),
                DSharpBinaryOperator.Divide => Instance.Divide(left, right),
                DSharpBinaryOperator.Mod => Instance.Mod(left, right),
                DSharpBinaryOperator.LogicalXor => Instance.Xor(left, right),
                DSharpBinaryOperator.Or => Instance.Or(left, right),
                DSharpBinaryOperator.And => Instance.And(left, right),
                DSharpBinaryOperator.ShiftLeft => Instance.ShiftLeft(left, right),
                DSharpBinaryOperator.ShiftRight => Instance.ShiftRight(left, right),
                DSharpBinaryOperator.LogicalAnd => Instance.LogicalAnd(left, right),
                DSharpBinaryOperator.LogicalOr => Instance.LogicalOr(left, right),
                DSharpBinaryOperator.LogicalEquals => Instance.LogicalEquals(left, right),
                DSharpBinaryOperator.LogicalNotEquals => Instance.LogicalNotEquals(left, right),
                DSharpBinaryOperator.LogicalLess => Instance.LogicalLess(left, right),
                DSharpBinaryOperator.LogicalLessOrEquals => Instance.LogicalLessOrEquals(left, right),
                DSharpBinaryOperator.LogicalGreater => Instance.LogicalGreater(left, right),
                DSharpBinaryOperator.LogicalGreaterOrEquals => Instance.LogicalGreaterOrEquals(left, right),
                _ => throw new ArgumentException($"Invalid binary operator \"{@operator}\"", nameof(@operator))
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumberKind GetKindAndConvert(DSharpNumber value, out DSharpNumber convertedValue)
        {
            var result = GetKind(value);
            convertedValue = Convert(value, result);

            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumberKind GetKindAndBigger(DSharpNumber left, DSharpNumber right, out DSharpLiteralType biggerType, out DSharpNumber convertedLeft, out DSharpNumber convertedRight)
        {
            biggerType = GetBiggerType(left, right);
            return GetKind(left, right, out convertedLeft, out convertedRight);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpLiteralType GetBiggerType(DSharpNumber left, DSharpNumber right)
        {
            return (DSharpLiteralType)Math.Max((byte)left.Type, (byte)right.Type);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumberKind GetKind(DSharpNumber left, DSharpNumber right, out DSharpNumber convertedLeft, out DSharpNumber convertedRight)
        {
            var leftKind = GetKind(left);
            var rightKind = GetKind(right);
            var result = (DSharpNumberKind)Math.Max((int)leftKind, (int)rightKind);

            convertedLeft = Convert(left, result);
            convertedRight = Convert(right, result);

            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumberKind GetKind(DSharpNumber number)
        {
            var type = number.Type;

            return type switch
            {
                DSharpLiteralType.Decimal => DSharpNumberKind.Decimal,
                DSharpLiteralType.Double => DSharpNumberKind.Double,
                DSharpLiteralType.Float => DSharpNumberKind.Double,
                DSharpLiteralType.NUInt => DSharpNumberKind.UInt64,
                DSharpLiteralType.ULong => DSharpNumberKind.UInt64,
                _ => DSharpNumberKind.Int64
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DSharpNumber Convert(DSharpNumber number, DSharpNumberKind kind)
        {
            return kind switch
            {
                DSharpNumberKind.Int64 => number.Convert<long>(),
                DSharpNumberKind.UInt64 => number.Convert<ulong>(),
                DSharpNumberKind.Double => number.Convert<double>(),
                DSharpNumberKind.Decimal => number.Convert<decimal>(),
                _ => number.Convert<long>(),
            };
        }

        #endregion
    }
}

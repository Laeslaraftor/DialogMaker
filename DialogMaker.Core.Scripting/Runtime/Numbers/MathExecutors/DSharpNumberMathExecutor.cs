using System.Runtime.CompilerServices;
using DialogMaker.Core.Scripting.Runtime.Executor;

namespace DialogMaker.Core.Scripting.Runtime.Numbers.MathExecutors
{
    internal static class DSharpNumberMathExecutor
    {
        #region Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber Add(ref DSharpNumber left, ref DSharpNumber right)
        {
            if (left._type == right._type)
            {
                return left._type switch
                {
                    DSharpLiteralType.Bool => unchecked(left._byte + right._byte) != 0,
                    DSharpLiteralType.Byte => unchecked(left._byte + right._byte),
                    DSharpLiteralType.SByte => unchecked(left._sbyte + right._sbyte),
                    DSharpLiteralType.Char => unchecked(left._char + right._char),
                    DSharpLiteralType.Short => unchecked(left._short + right._short),
                    DSharpLiteralType.UShort => unchecked(left._ushort + right._ushort),
                    DSharpLiteralType.Int => unchecked(left._int + right._int),
                    DSharpLiteralType.UInt => unchecked(left._uint + right._uint),
                    DSharpLiteralType.Long => unchecked(left._long + right._long),
                    DSharpLiteralType.ULong => unchecked(left._ulong + right._ulong),
                    DSharpLiteralType.NInt => unchecked(left._nint + right._nint),
                    DSharpLiteralType.NUInt => unchecked(left._nuint + right._nuint),
                    DSharpLiteralType.Float => left._float + right._float,
                    DSharpLiteralType.Double => left._double + right._double,
                    DSharpLiteralType.Decimal => left._decimal + right._decimal,
                    _ => throw new ArgumentException($"Invalid number type: {left._type}"),
                };
            }

            var kind = GetKindAndBigger(ref left, ref right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[(int)kind].Add(ref kindLeft, ref kindRight).Convert(bigger);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber Subtract(ref DSharpNumber left, ref DSharpNumber right)
        {
            if (left._type == right._type)
            {
                return left._type switch
                {
                    DSharpLiteralType.Bool => unchecked(left._byte - right._byte) != 0,
                    DSharpLiteralType.Byte => unchecked(left._byte - right._byte),
                    DSharpLiteralType.SByte => unchecked(left._sbyte - right._sbyte),
                    DSharpLiteralType.Char => unchecked(left._char - right._char),
                    DSharpLiteralType.Short => unchecked(left._short - right._short),
                    DSharpLiteralType.UShort => unchecked(left._ushort - right._ushort),
                    DSharpLiteralType.Int => unchecked(left._int - right._int),
                    DSharpLiteralType.UInt => unchecked(left._uint - right._uint),
                    DSharpLiteralType.Long => unchecked(left._long - right._long),
                    DSharpLiteralType.ULong => unchecked(left._ulong - right._ulong),
                    DSharpLiteralType.NInt => unchecked(left._nint - right._nint),
                    DSharpLiteralType.NUInt => unchecked(left._nuint - right._nuint),
                    DSharpLiteralType.Float => left._float - right._float,
                    DSharpLiteralType.Double => left._double - right._double,
                    DSharpLiteralType.Decimal => left._decimal - right._decimal,
                    _ => throw new ArgumentException($"Invalid number type: {left._type}"),
                };
            }

            var kind = GetKindAndBigger(ref left, ref right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[(int)kind].Subtract(ref kindLeft, ref kindRight).Convert(bigger);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber Mod(ref DSharpNumber left, ref DSharpNumber right)
        {
            if (left._type == right._type)
            {
                return left._type switch
                {
                    DSharpLiteralType.Bool => unchecked(left._byte % right._byte) != 0,
                    DSharpLiteralType.Byte => unchecked(left._byte % right._byte),
                    DSharpLiteralType.SByte => unchecked(left._sbyte % right._sbyte),
                    DSharpLiteralType.Char => unchecked(left._char % right._char),
                    DSharpLiteralType.Short => unchecked(left._short % right._short),
                    DSharpLiteralType.UShort => unchecked(left._ushort % right._ushort),
                    DSharpLiteralType.Int => unchecked(left._int % right._int),
                    DSharpLiteralType.UInt => unchecked(left._uint % right._uint),
                    DSharpLiteralType.Long => unchecked(left._long % right._long),
                    DSharpLiteralType.ULong => unchecked(left._ulong % right._ulong),
                    DSharpLiteralType.NInt => unchecked(left._nint % right._nint),
                    DSharpLiteralType.NUInt => unchecked(left._nuint % right._nuint),
                    DSharpLiteralType.Float => left._float % right._float,
                    DSharpLiteralType.Double => left._double % right._double,
                    DSharpLiteralType.Decimal => left._decimal % right._decimal,
                    _ => throw new ArgumentException($"Invalid number type: {left._type}"),
                };
            }

            var kind = GetKindAndBigger(ref left, ref right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[(int)kind].Mod(ref kindLeft, ref kindRight).Convert(bigger);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber Multiply(ref DSharpNumber left, ref DSharpNumber right)
        {
            if (left._type == right._type)
            {
                return left._type switch
                {
                    DSharpLiteralType.Bool => unchecked(left._byte * right._byte) != 0,
                    DSharpLiteralType.Byte => unchecked(left._byte * right._byte),
                    DSharpLiteralType.SByte => unchecked(left._sbyte * right._sbyte),
                    DSharpLiteralType.Char => unchecked(left._char * right._char),
                    DSharpLiteralType.Short => unchecked(left._short * right._short),
                    DSharpLiteralType.UShort => unchecked(left._ushort * right._ushort),
                    DSharpLiteralType.Int => unchecked(left._int * right._int),
                    DSharpLiteralType.UInt => unchecked(left._uint * right._uint),
                    DSharpLiteralType.Long => unchecked(left._long * right._long),
                    DSharpLiteralType.ULong => unchecked(left._ulong * right._ulong),
                    DSharpLiteralType.NInt => unchecked(left._nint * right._nint),
                    DSharpLiteralType.NUInt => unchecked(left._nuint * right._nuint),
                    DSharpLiteralType.Float => left._float * right._float,
                    DSharpLiteralType.Double => left._double * right._double,
                    DSharpLiteralType.Decimal => left._decimal * right._decimal,
                    _ => throw new ArgumentException($"Invalid number type: {left._type}"),
                };
            }

            var kind = GetKindAndBigger(ref left, ref right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[(int)kind].Multiply(ref kindLeft, ref kindRight).Convert(bigger);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber Divide(ref DSharpNumber left, ref DSharpNumber right)
        {
            if (left._type == right._type)
            {
                return left._type switch
                {
                    DSharpLiteralType.Bool => unchecked(left._byte / right._byte) != 0,
                    DSharpLiteralType.Byte => unchecked(left._byte / right._byte),
                    DSharpLiteralType.SByte => unchecked(left._sbyte / right._sbyte),
                    DSharpLiteralType.Char => unchecked(left._char / right._char),
                    DSharpLiteralType.Short => unchecked(left._short / right._short),
                    DSharpLiteralType.UShort => unchecked(left._ushort / right._ushort),
                    DSharpLiteralType.Int => unchecked(left._int / right._int),
                    DSharpLiteralType.UInt => unchecked(left._uint / right._uint),
                    DSharpLiteralType.Long => unchecked(left._long / right._long),
                    DSharpLiteralType.ULong => unchecked(left._ulong / right._ulong),
                    DSharpLiteralType.NInt => unchecked(left._nint / right._nint),
                    DSharpLiteralType.NUInt => unchecked(left._nuint / right._nuint),
                    DSharpLiteralType.Float => left._float / right._float,
                    DSharpLiteralType.Double => left._double / right._double,
                    DSharpLiteralType.Decimal => left._decimal / right._decimal,
                    _ => throw new ArgumentException($"Invalid number type: {left._type}"),
                };
            }

            var kind = GetKindAndBigger(ref left, ref right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[(int)kind].Divide(ref kindLeft, ref kindRight).Convert(bigger);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber Xor(ref DSharpNumber left, ref DSharpNumber right)
        {
            if (left._type == right._type)
            {
                return left._type switch
                {
                    DSharpLiteralType.Bool => left._bool ^ right._bool,
                    DSharpLiteralType.Byte => unchecked(left._byte ^ right._byte),
                    DSharpLiteralType.SByte => unchecked(left._sbyte ^ right._sbyte),
                    DSharpLiteralType.Char => unchecked(left._char ^ right._char),
                    DSharpLiteralType.Short => unchecked(left._short ^ right._short),
                    DSharpLiteralType.UShort => unchecked(left._ushort ^ right._ushort),
                    DSharpLiteralType.Int => unchecked(left._int ^ right._int),
                    DSharpLiteralType.UInt => unchecked(left._uint ^ right._uint),
                    DSharpLiteralType.Long => unchecked(left._long ^ right._long),
                    DSharpLiteralType.ULong => unchecked(left._ulong ^ right._ulong),
                    DSharpLiteralType.NInt => unchecked(left._nint ^ right._nint),
                    DSharpLiteralType.NUInt => unchecked(left._nuint ^ right._nuint),
                    DSharpLiteralType.Float => left._float ^ right._float,
                    DSharpLiteralType.Double => left._double ^ right._double,
                    DSharpLiteralType.Decimal => left._decimal ^ right._decimal,
                    _ => throw new ArgumentException($"Invalid number type: {left._type}"),
                };
            }

            var kind = GetKindAndBigger(ref left, ref right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[(int)kind].Xor(ref kindLeft, ref kindRight).Convert(bigger);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber Or(ref DSharpNumber left, ref DSharpNumber right)
        {
            if (left._type == right._type)
            {
                return left._type switch
                {
                    DSharpLiteralType.Bool => left._bool | right._bool,
                    DSharpLiteralType.Byte => unchecked(left._byte | right._byte),
                    DSharpLiteralType.SByte => unchecked(left._sbyte | right._sbyte),
                    DSharpLiteralType.Char => unchecked(left._char | right._char),
                    DSharpLiteralType.Short => unchecked(left._short | right._short),
                    DSharpLiteralType.UShort => unchecked(left._ushort | right._ushort),
                    DSharpLiteralType.Int => unchecked(left._int | right._int),
                    DSharpLiteralType.UInt => unchecked(left._uint | right._uint),
                    DSharpLiteralType.Long => unchecked(left._long | right._long),
                    DSharpLiteralType.ULong => unchecked(left._ulong | right._ulong),
                    DSharpLiteralType.NInt => unchecked(left._nint | right._nint),
                    DSharpLiteralType.NUInt => unchecked(left._nuint | right._nuint),
                    DSharpLiteralType.Float => left._float | right._float,
                    DSharpLiteralType.Double => left._double | right._double,
                    DSharpLiteralType.Decimal => left._decimal | right._decimal,
                    _ => throw new ArgumentException($"Invalid number type: {left._type}"),
                };
            }

            var kind = GetKindAndBigger(ref left, ref right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[(int)kind].Or(ref kindLeft, ref kindRight).Convert(bigger);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber And(ref DSharpNumber left, ref DSharpNumber right)
        {
            if (left._type == right._type)
            {
                return left._type switch
                {
                    DSharpLiteralType.Bool => left._bool & right._bool,
                    DSharpLiteralType.Byte => unchecked(left._byte & right._byte),
                    DSharpLiteralType.SByte => unchecked(left._sbyte & right._sbyte),
                    DSharpLiteralType.Char => unchecked(left._char & right._char),
                    DSharpLiteralType.Short => unchecked(left._short & right._short),
                    DSharpLiteralType.UShort => unchecked(left._ushort & right._ushort),
                    DSharpLiteralType.Int => unchecked(left._int & right._int),
                    DSharpLiteralType.UInt => unchecked(left._uint & right._uint),
                    DSharpLiteralType.Long => unchecked(left._long & right._long),
                    DSharpLiteralType.ULong => unchecked(left._ulong & right._ulong),
                    DSharpLiteralType.NInt => unchecked(left._nint & right._nint),
                    DSharpLiteralType.NUInt => unchecked(left._nuint & right._nuint),
                    DSharpLiteralType.Float => left._float & right._float,
                    DSharpLiteralType.Double => left._double & right._double,
                    DSharpLiteralType.Decimal => left._decimal & right._decimal,
                    _ => throw new ArgumentException($"Invalid number type: {left._type}"),
                };
            }

            var kind = GetKindAndBigger(ref left, ref right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[(int)kind].And(ref kindLeft, ref kindRight).Convert(bigger);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber ShiftLeft(ref DSharpNumber left, ref DSharpNumber right)
        {
            if (left._type == right._type)
            {
                return left._type switch
                {
                    DSharpLiteralType.Bool => (left._byte << right._byte) != 0,
                    DSharpLiteralType.Byte => unchecked(left._byte << right._byte),
                    DSharpLiteralType.SByte => unchecked(left._sbyte << right._sbyte),
                    DSharpLiteralType.Char => unchecked(left._char << right._char),
                    DSharpLiteralType.Short => unchecked(left._short << right._short),
                    DSharpLiteralType.UShort => unchecked(left._ushort << right._ushort),
                    DSharpLiteralType.Int => unchecked(left._int << right._int),
                    DSharpLiteralType.UInt => unchecked(left._uint << unchecked((int)right._uint)),
                    DSharpLiteralType.Long => unchecked(left._long << unchecked((int)right._long)),
                    DSharpLiteralType.ULong => unchecked(left._ulong << unchecked((int)right._ulong)),
                    DSharpLiteralType.NInt => unchecked(left._nint << unchecked((int)right._nint)),
                    DSharpLiteralType.NUInt => unchecked(left._nuint << unchecked((int)right._nuint)),
                    DSharpLiteralType.Float => left._float << right._float,
                    DSharpLiteralType.Double => left._double << right._double,
                    DSharpLiteralType.Decimal => left._decimal << right._decimal,
                    _ => throw new ArgumentException($"Invalid number type: {left._type}"),
                };
            }

            var kind = GetKindAndBigger(ref left, ref right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[(int)kind].ShiftLeft(ref kindLeft, ref kindRight).Convert(bigger);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber ShiftRight(ref DSharpNumber left, ref DSharpNumber right)
        {
            if (left._type == right._type)
            {
                return left._type switch
                {
                    DSharpLiteralType.Bool => (left._byte >> right._byte) != 0,
                    DSharpLiteralType.Byte => unchecked(left._byte >> right._byte),
                    DSharpLiteralType.SByte => unchecked(left._sbyte >> right._sbyte),
                    DSharpLiteralType.Char => unchecked(left._char >> right._char),
                    DSharpLiteralType.Short => unchecked(left._short >> right._short),
                    DSharpLiteralType.UShort => unchecked(left._ushort >> right._ushort),
                    DSharpLiteralType.Int => unchecked(left._int >> right._int),
                    DSharpLiteralType.UInt => unchecked(left._uint >> unchecked((int)right._uint)),
                    DSharpLiteralType.Long => unchecked(left._long >> unchecked((int)right._long)),
                    DSharpLiteralType.ULong => unchecked(left._ulong >> unchecked((int)right._ulong)),
                    DSharpLiteralType.NInt => unchecked(left._nint >> unchecked((int)right._nint)),
                    DSharpLiteralType.NUInt => unchecked(left._nuint >> unchecked((int)right._nuint)),
                    DSharpLiteralType.Float => left._float >> right._float,
                    DSharpLiteralType.Double => left._double >> right._double,
                    DSharpLiteralType.Decimal => left._decimal >> right._decimal,
                    _ => throw new ArgumentException($"Invalid number type: {left._type}"),
                };
            }

            var kind = GetKindAndBigger(ref left, ref right, out var bigger, out var kindLeft, out var kindRight);
            return _mathExecutors[(int)kind].ShiftRight(ref kindLeft, ref kindRight).Convert(bigger);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber LogicalAnd(ref DSharpNumber left, ref DSharpNumber right)
        {
            if (left._type == right._type)
            {
                return left._type switch
                {
                    DSharpLiteralType.Bool => left._bool && right._bool,
                    DSharpLiteralType.Byte => unchecked(left._byte * right._byte) != 0,
                    DSharpLiteralType.SByte => unchecked(left._sbyte * right._sbyte) != 0,
                    DSharpLiteralType.Char => unchecked(left._char * right._char) != 0,
                    DSharpLiteralType.Short => unchecked(left._short * right._short) != 0,
                    DSharpLiteralType.UShort => unchecked(left._ushort * right._ushort) != 0,
                    DSharpLiteralType.Int => unchecked(left._int * right._int) != 0,
                    DSharpLiteralType.UInt => unchecked(left._uint * right._uint) != 0,
                    DSharpLiteralType.Long => unchecked(left._long * right._long) != 0,
                    DSharpLiteralType.ULong => unchecked(left._ulong * right._ulong) != 0,
                    DSharpLiteralType.NInt => unchecked(left._nint * right._nint) != 0,
                    DSharpLiteralType.NUInt => unchecked(left._nuint * right._nuint) != 0,
                    DSharpLiteralType.Float => (left._float * right._float) != 0,
                    DSharpLiteralType.Double => (left._double * right._double) != 0,
                    DSharpLiteralType.Decimal => (left._decimal * right._decimal) != 0,
                    _ => throw new ArgumentException($"Invalid number type: {left._type}"),
                };
            }

            var kind = GetKind(ref left, ref right, out var kindLeft, out var kindRight);
            return _mathExecutors[(int)kind].LogicalAnd(ref kindLeft, ref kindRight);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber LogicalOr(ref DSharpNumber left, ref DSharpNumber right)
        {
            if (left._type == right._type)
            {
                return left._type switch
                {
                    DSharpLiteralType.Bool => left._bool || right._bool,
                    DSharpLiteralType.Byte => unchecked(left._byte + right._byte) != 0,
                    DSharpLiteralType.SByte => unchecked(left._sbyte + right._sbyte) != 0,
                    DSharpLiteralType.Char => unchecked(left._char + right._char) != 0,
                    DSharpLiteralType.Short => unchecked(left._short + right._short) != 0,
                    DSharpLiteralType.UShort => unchecked(left._ushort + right._ushort) != 0,
                    DSharpLiteralType.Int => unchecked(left._int + right._int) != 0,
                    DSharpLiteralType.UInt => unchecked(left._uint + right._uint) != 0,
                    DSharpLiteralType.Long => unchecked(left._long + right._long) != 0,
                    DSharpLiteralType.ULong => unchecked(left._ulong + right._ulong) != 0,
                    DSharpLiteralType.NInt => unchecked(left._nint + right._nint) != 0,
                    DSharpLiteralType.NUInt => unchecked(left._nuint + right._nuint) != 0,
                    DSharpLiteralType.Float => (left._float + right._float) != 0,
                    DSharpLiteralType.Double => (left._double + right._double) != 0,
                    DSharpLiteralType.Decimal => (left._decimal + right._decimal) != 0,
                    _ => throw new ArgumentException($"Invalid number type: {left._type}"),
                };
            }

            var kind = GetKind(ref left, ref right, out var kindLeft, out var kindRight);
            return _mathExecutors[(int)kind].LogicalOr(ref kindLeft, ref kindRight);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber Equals(ref DSharpNumber left, ref DSharpNumber right)
        {
            if (left._type == right._type)
            {
                return left._type switch
                {
                    DSharpLiteralType.Bool => left._bool == right._bool,
                    DSharpLiteralType.Byte => left._byte == right._byte,
                    DSharpLiteralType.SByte => left._sbyte == right._sbyte,
                    DSharpLiteralType.Char => left._char == right._char,
                    DSharpLiteralType.Short => left._short == right._short,
                    DSharpLiteralType.UShort => left._ushort == right._ushort,
                    DSharpLiteralType.Int => left._int == right._int,
                    DSharpLiteralType.UInt => left._uint == right._uint,
                    DSharpLiteralType.Long => left._long == right._long,
                    DSharpLiteralType.ULong => left._ulong == right._ulong,
                    DSharpLiteralType.NInt => left._nint == right._nint,
                    DSharpLiteralType.NUInt => left._nuint == right._nuint,
                    DSharpLiteralType.Float => left._float == right._float,
                    DSharpLiteralType.Double => left._double == right._double,
                    DSharpLiteralType.Decimal => left._decimal == right._decimal,
                    _ => throw new ArgumentException($"Invalid number type: {left._type}"),
                };
            }

            var kind = GetKind(ref left, ref right, out var kindLeft, out var kindRight);
            return _mathExecutors[(int)kind].LogicalEquals(ref kindLeft, ref kindRight);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber NotEquals(ref DSharpNumber left, ref DSharpNumber right)
        {
            if (left._type == right._type)
            {
                return left._type switch
                {
                    DSharpLiteralType.Bool => left._bool != right._bool,
                    DSharpLiteralType.Byte => left._byte != right._byte,
                    DSharpLiteralType.SByte => left._sbyte != right._sbyte,
                    DSharpLiteralType.Char => left._char != right._char,
                    DSharpLiteralType.Short => left._short != right._short,
                    DSharpLiteralType.UShort => left._ushort != right._ushort,
                    DSharpLiteralType.Int => left._int != right._int,
                    DSharpLiteralType.UInt => left._uint != right._uint,
                    DSharpLiteralType.Long => left._long != right._long,
                    DSharpLiteralType.ULong => left._ulong != right._ulong,
                    DSharpLiteralType.NInt => left._nint != right._nint,
                    DSharpLiteralType.NUInt => left._nuint != right._nuint,
                    DSharpLiteralType.Float => left._float != right._float,
                    DSharpLiteralType.Double => left._double != right._double,
                    DSharpLiteralType.Decimal => left._decimal != right._decimal,
                    _ => throw new ArgumentException($"Invalid number type: {left._type}"),
                };
            }

            var kind = GetKind(ref left, ref right, out var kindLeft, out var kindRight);
            return _mathExecutors[(int)kind].LogicalNotEquals(ref kindLeft, ref kindRight);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber Less(ref DSharpNumber left, ref DSharpNumber right)
        {
            if (left._type == right._type)
            {
                return left._type switch
                {
                    DSharpLiteralType.Bool => left._byte < right._byte,
                    DSharpLiteralType.Byte => left._byte < right._byte,
                    DSharpLiteralType.SByte => left._sbyte < right._sbyte,
                    DSharpLiteralType.Char => left._char < right._char,
                    DSharpLiteralType.Short => left._short < right._short,
                    DSharpLiteralType.UShort => left._ushort < right._ushort,
                    DSharpLiteralType.Int => left._int < right._int,
                    DSharpLiteralType.UInt => left._uint < right._uint,
                    DSharpLiteralType.Long => left._long < right._long,
                    DSharpLiteralType.ULong => left._ulong < right._ulong,
                    DSharpLiteralType.NInt => left._nint < right._nint,
                    DSharpLiteralType.NUInt => left._nuint < right._nuint,
                    DSharpLiteralType.Float => left._float < right._float,
                    DSharpLiteralType.Double => left._double < right._double,
                    DSharpLiteralType.Decimal => left._decimal < right._decimal,
                    _ => throw new ArgumentException($"Invalid number type: {left._type}"),
                };
            }

            var kind = GetKind(ref left, ref right, out var kindLeft, out var kindRight);
            return _mathExecutors[(int)kind].LogicalLess(ref kindLeft, ref kindRight);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber LessOrEqual(ref DSharpNumber left, ref DSharpNumber right)
        {
            if (left._type == right._type)
            {
                return left._type switch
                {
                    DSharpLiteralType.Bool => left._byte <= right._byte,
                    DSharpLiteralType.Byte => left._byte <= right._byte,
                    DSharpLiteralType.SByte => left._sbyte <= right._sbyte,
                    DSharpLiteralType.Char => left._char <= right._char,
                    DSharpLiteralType.Short => left._short <= right._short,
                    DSharpLiteralType.UShort => left._ushort <= right._ushort,
                    DSharpLiteralType.Int => left._int <= right._int,
                    DSharpLiteralType.UInt => left._uint <= right._uint,
                    DSharpLiteralType.Long => left._long <= right._long,
                    DSharpLiteralType.ULong => left._ulong <= right._ulong,
                    DSharpLiteralType.NInt => left._nint <= right._nint,
                    DSharpLiteralType.NUInt => left._nuint <= right._nuint,
                    DSharpLiteralType.Float => left._float <= right._float,
                    DSharpLiteralType.Double => left._double <= right._double,
                    DSharpLiteralType.Decimal => left._decimal <= right._decimal,
                    _ => throw new ArgumentException($"Invalid number type: {left._type}"),
                };
            }

            var kind = GetKind(ref left, ref right, out var kindLeft, out var kindRight);
            return _mathExecutors[(int)kind].LogicalLessOrEquals(ref kindLeft, ref kindRight);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber Greater(ref DSharpNumber left, ref DSharpNumber right)
        {
            if (left._type == right._type)
            {
                return left._type switch
                {
                    DSharpLiteralType.Bool => left._byte > right._byte,
                    DSharpLiteralType.Byte => left._byte > right._byte,
                    DSharpLiteralType.SByte => left._sbyte > right._sbyte,
                    DSharpLiteralType.Char => left._char > right._char,
                    DSharpLiteralType.Short => left._short > right._short,
                    DSharpLiteralType.UShort => left._ushort > right._ushort,
                    DSharpLiteralType.Int => left._int > right._int,
                    DSharpLiteralType.UInt => left._uint > right._uint,
                    DSharpLiteralType.Long => left._long > right._long,
                    DSharpLiteralType.ULong => left._ulong > right._ulong,
                    DSharpLiteralType.NInt => left._nint > right._nint,
                    DSharpLiteralType.NUInt => left._nuint > right._nuint,
                    DSharpLiteralType.Float => left._float > right._float,
                    DSharpLiteralType.Double => left._double > right._double,
                    DSharpLiteralType.Decimal => left._decimal > right._decimal,
                    _ => throw new ArgumentException($"Invalid number type: {left._type}"),
                };
            }

            var kind = GetKind(ref left, ref right, out var kindLeft, out var kindRight);
            return _mathExecutors[(int)kind].LogicalGreater(ref kindLeft, ref kindRight);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber GreaterOrEqual(ref DSharpNumber left, ref DSharpNumber right)
        {
            if (left._type == right._type)
            {
                return left._type switch
                {
                    DSharpLiteralType.Bool => left._byte >= right._byte,
                    DSharpLiteralType.Byte => left._byte >= right._byte,
                    DSharpLiteralType.SByte => left._sbyte >= right._sbyte,
                    DSharpLiteralType.Char => left._char >= right._char,
                    DSharpLiteralType.Short => left._short >= right._short,
                    DSharpLiteralType.UShort => left._ushort >= right._ushort,
                    DSharpLiteralType.Int => left._int >= right._int,
                    DSharpLiteralType.UInt => left._uint >= right._uint,
                    DSharpLiteralType.Long => left._long >= right._long,
                    DSharpLiteralType.ULong => left._ulong >= right._ulong,
                    DSharpLiteralType.NInt => left._nint >= right._nint,
                    DSharpLiteralType.NUInt => left._nuint >= right._nuint,
                    DSharpLiteralType.Float => left._float >= right._float,
                    DSharpLiteralType.Double => left._double >= right._double,
                    DSharpLiteralType.Decimal => left._decimal >= right._decimal,
                    _ => throw new ArgumentException($"Invalid number type: {left._type}"),
                };
            }

            var kind = GetKind(ref left, ref right, out var kindLeft, out var kindRight);
            return _mathExecutors[(int)kind].LogicalGreaterOrEquals(ref kindLeft, ref kindRight);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static DSharpNumber Invert(ref DSharpNumber value)
        {
            return value._type switch
            {
                DSharpLiteralType.Bool => !value._bool,
                DSharpLiteralType.Byte => -value._byte,
                DSharpLiteralType.SByte => -value._sbyte,
                DSharpLiteralType.Char => -value._char,
                DSharpLiteralType.Short => -value._short,
                DSharpLiteralType.UShort => -value._ushort,
                DSharpLiteralType.Int => -value._int,
                DSharpLiteralType.UInt => -value._uint,
                DSharpLiteralType.Long => -value._long,
                DSharpLiteralType.ULong => value._ulong == 0 ? ulong.MaxValue : 0UL,
                DSharpLiteralType.NInt => -value._nint,
                DSharpLiteralType.NUInt => (nuint)(value._nuint == 0 ? (sizeof(nuint) == sizeof(ulong) ? ulong.MaxValue : uint.MaxValue) : 0),
                DSharpLiteralType.Float => -value._float,
                DSharpLiteralType.Double => -value._double,
                DSharpLiteralType.Decimal => -value._decimal,
                _ => throw new ArgumentException($"Invalid number type: {value._type}"),
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber Not(ref DSharpNumber value)
        {
            return Invert(ref value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber Increment(ref DSharpNumber value)
        {
            return value._type switch
            {
                DSharpLiteralType.Bool => true,
                DSharpLiteralType.Byte => unchecked(value._byte + 1),
                DSharpLiteralType.SByte => unchecked(value._sbyte + 1),
                DSharpLiteralType.Char => unchecked(value._char + 1),
                DSharpLiteralType.Short => unchecked(value._short + 1),
                DSharpLiteralType.UShort => unchecked(value._ushort + 1),
                DSharpLiteralType.Int => unchecked(value._int + 1),
                DSharpLiteralType.UInt => unchecked(value._uint + 1),
                DSharpLiteralType.Long => unchecked(value._long + 1),
                DSharpLiteralType.ULong => unchecked(value._ulong + 1),
                DSharpLiteralType.NInt => unchecked(value._nint + 1),
                DSharpLiteralType.NUInt => unchecked(value._nuint + 1),
                DSharpLiteralType.Float => unchecked(value._float + 1),
                DSharpLiteralType.Double => unchecked(value._double + 1),
                DSharpLiteralType.Decimal => unchecked(value._decimal + 1),
                _ => throw new ArgumentException($"Invalid number type: {value._type}"),
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber Decrement(ref DSharpNumber value)
        {
            return value._type switch
            {
                DSharpLiteralType.Bool => false,
                DSharpLiteralType.Byte => unchecked(value._byte - 1),
                DSharpLiteralType.SByte => unchecked(value._sbyte - 1),
                DSharpLiteralType.Char => unchecked(value._char - 1),
                DSharpLiteralType.Short => unchecked(value._short - 1),
                DSharpLiteralType.UShort => unchecked(value._ushort - 1),
                DSharpLiteralType.Int => unchecked(value._int - 1),
                DSharpLiteralType.UInt => unchecked(value._uint - 1),
                DSharpLiteralType.Long => unchecked(value._long - 1),
                DSharpLiteralType.ULong => unchecked(value._ulong - 1),
                DSharpLiteralType.NInt => unchecked(value._nint - 1),
                DSharpLiteralType.NUInt => unchecked(value._nuint - 1),
                DSharpLiteralType.Float => unchecked(value._float - 1),
                DSharpLiteralType.Double => unchecked(value._double - 1),
                DSharpLiteralType.Decimal => unchecked(value._decimal - 1),
                _ => throw new ArgumentException($"Invalid number type: {value._type}"),
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber BitInvert(ref DSharpNumber value)
        {
            return value._type switch
            {
                DSharpLiteralType.Bool => !value._bool,
                DSharpLiteralType.Byte => ~value._byte,
                DSharpLiteralType.SByte => ~value._sbyte,
                DSharpLiteralType.Char => ~value._char,
                DSharpLiteralType.Short => ~value._short,
                DSharpLiteralType.UShort => ~value._ushort,
                DSharpLiteralType.Int => ~value._int,
                DSharpLiteralType.UInt => ~value._uint,
                DSharpLiteralType.Long => ~value._long,
                DSharpLiteralType.ULong => ~value._ulong,
                DSharpLiteralType.NInt => ~value._nint,
                DSharpLiteralType.NUInt => ~value._nuint,
                DSharpLiteralType.Float => ~value._float,
                DSharpLiteralType.Double => ~value._double,
                DSharpLiteralType.Decimal => ~value._decimal,
                _ => throw new ArgumentException($"Invalid number type: {value._type}"),
            };
        }

        #endregion

        #region Static

        private static readonly IDSharpNumberMathExecutor[] _mathExecutors =
        {
            DSharpInt64MathExecutor.Instance,
            DSharpUInt64MathExecutor.Instance,
            DSharpDoubleMathExecutor.Instance,
            DSharpDecimalMathExecutor.Instance,
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber Perform(ref DSharpNumber value, DSharpUnaryOperator @operator)
        {
            return @operator switch
            {
                DSharpUnaryOperator.Minus => Invert(ref value),
                DSharpUnaryOperator.Not => Not(ref value),
                DSharpUnaryOperator.Increment => Increment(ref value),
                DSharpUnaryOperator.Decrement => Decrement(ref value),
                DSharpUnaryOperator.BitInvert => BitInvert(ref value),
                _ => throw new ArgumentException($"Invalid unary operator \"{@operator}\"", nameof(@operator))
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber Perform(ref DSharpNumber left, ref DSharpNumber right, DSharpBinaryOperator @operator)
        {
            return @operator switch
            {
                DSharpBinaryOperator.Plus => Add(ref left, ref right),
                DSharpBinaryOperator.Minus => Subtract(ref left, ref right),
                DSharpBinaryOperator.Multiply => Multiply(ref left, ref right),
                DSharpBinaryOperator.Divide => Divide(ref left, ref right),
                DSharpBinaryOperator.Mod => Mod(ref left, ref right),
                DSharpBinaryOperator.LogicalXor => Xor(ref left, ref right),
                DSharpBinaryOperator.Or => Or(ref left, ref right),
                DSharpBinaryOperator.And => And(ref left, ref right),
                DSharpBinaryOperator.ShiftLeft => ShiftLeft(ref left, ref right),
                DSharpBinaryOperator.ShiftRight => ShiftRight(ref left, ref right),
                DSharpBinaryOperator.LogicalAnd => LogicalAnd(ref left, ref right),
                DSharpBinaryOperator.LogicalOr => LogicalOr(ref left, ref right),
                DSharpBinaryOperator.LogicalEquals => Equals(ref left, ref right),
                DSharpBinaryOperator.LogicalNotEquals => NotEquals(ref left, ref right),
                DSharpBinaryOperator.LogicalLess => Less(ref left, ref right),
                DSharpBinaryOperator.LogicalLessOrEquals => LessOrEqual(ref left, ref right),
                DSharpBinaryOperator.LogicalGreater => Greater(ref left, ref right),
                DSharpBinaryOperator.LogicalGreaterOrEquals => GreaterOrEqual(ref left, ref right),
                _ => throw new ArgumentException($"Invalid binary operator \"{@operator}\"", nameof(@operator))
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumberKind GetKindAndConvert(ref DSharpNumber value, out DSharpNumber convertedValue)
        {
            var result = GetKind(ref value);
            convertedValue = Convert(ref value, result);

            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumberKind GetKindAndBigger(ref DSharpNumber left, ref DSharpNumber right, out DSharpLiteralType biggerType, out DSharpNumber convertedLeft, out DSharpNumber convertedRight)
        {
            biggerType = GetBiggerType(ref left, ref right);
            return GetKind(ref left, ref right, out convertedLeft, out convertedRight);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpLiteralType GetBiggerType(ref DSharpNumber left, ref DSharpNumber right)
        {
            return (DSharpLiteralType)Math.Max((byte)left.Type, (byte)right.Type);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumberKind GetKind(ref DSharpNumber left, ref DSharpNumber right, out DSharpNumber convertedLeft, out DSharpNumber convertedRight)
        {
            var leftKind = GetKind(ref left);
            var rightKind = GetKind(ref right);
            var result = (DSharpNumberKind)Math.Max((int)leftKind, (int)rightKind);

            convertedLeft = Convert(ref left, result);
            convertedRight = Convert(ref right, result);

            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumberKind GetKind(ref DSharpNumber number)
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
        private static DSharpNumber Convert(ref DSharpNumber number, DSharpNumberKind kind)
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

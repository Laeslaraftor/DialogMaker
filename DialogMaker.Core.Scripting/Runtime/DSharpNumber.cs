using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Runtime.Numbers.MathExecutors;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Structure that represents unmanaged number
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 17)]
    public unsafe struct DSharpNumber : IEquatable<DSharpNumber>
    {
        /// <summary>
        /// Current number type
        /// </summary>
        public readonly DSharpLiteralType Type => _type;
        /// <summary>
        /// Current number size in bytes
        /// </summary>
        public readonly int Size => Sizes[_type];

        #region Fields

        [FieldOffset(16)]
        private DSharpLiteralType _type;
        [FieldOffset(0)]
        private byte _byte;
        [FieldOffset(0)]
        private sbyte _sbyte;
        [FieldOffset(0)]
        private bool _bool;
        [FieldOffset(0)]
        private short _short;
        [FieldOffset(0)]
        private ushort _ushort;
        [FieldOffset(0)]
        private char _char;
        [FieldOffset(0)]
        private int _int;
        [FieldOffset(0)]
        private uint _uint;
        [FieldOffset(0)]
        private long _long;
        [FieldOffset(0)]
        private ulong _ulong;
        [FieldOffset(0)]
        private nint _nint;
        [FieldOffset(0)]
        private nuint _nuint;
        [FieldOffset(0)]
        private float _float;
        [FieldOffset(0)]
        private double _double;
        [FieldOffset(0)]
        private decimal _decimal;

        #endregion

        #region Controls

        /// <summary>
        /// Read number. This method reads raw data and do not convert numbers
        /// </summary>
        /// <typeparam name="T">Required number type</typeparam>
        /// <returns>Number</returns>
        /// <exception cref="InvalidOperationException">Invalid number type</exception>
        public T Read<T>() where T : unmanaged
        {
            var type = _type;

            return type switch
            {
                DSharpLiteralType.Byte => Unsafe.As<byte, T>(ref _byte),
                DSharpLiteralType.SByte => Unsafe.As<sbyte, T>(ref _sbyte),
                DSharpLiteralType.Bool => Unsafe.As<bool, T>(ref _bool),
                DSharpLiteralType.Short => Unsafe.As<short, T>(ref _short),
                DSharpLiteralType.UShort => Unsafe.As<ushort, T>(ref _ushort),
                DSharpLiteralType.Char => Unsafe.As<char, T>(ref _char),
                DSharpLiteralType.Int => Unsafe.As<int, T>(ref _int),
                DSharpLiteralType.UInt => Unsafe.As<uint, T>(ref _uint),
                DSharpLiteralType.Long => Unsafe.As<long, T>(ref _long),
                DSharpLiteralType.ULong => Unsafe.As<ulong, T>(ref _ulong),
                DSharpLiteralType.NInt => Unsafe.As<nint, T>(ref _nint),
                DSharpLiteralType.NUInt => Unsafe.As<nuint, T>(ref _nuint),
                DSharpLiteralType.Float => Unsafe.As<float, T>(ref _float),
                DSharpLiteralType.Double => Unsafe.As<double, T>(ref _double),
                DSharpLiteralType.Decimal => Unsafe.As<decimal, T>(ref _decimal),
                _ => throw new InvalidOperationException($"Invalid number type: {type}")
            };
        }
        /// <summary>
        /// Write number value
        /// </summary>
        /// <typeparam name="T">Number type for writing</typeparam>
        /// <param name="value">Value that need to write</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(T value) where T : unmanaged
        {
            var type = _type;

            switch(type)
            {
                case DSharpLiteralType.Byte:
                    _byte = Unsafe.As<T, byte>(ref value);
                    break;
                case DSharpLiteralType.SByte:
                    _sbyte = Unsafe.As<T, sbyte>(ref value);
                    break;
                case DSharpLiteralType.Bool:
                    _bool = Unsafe.As<T, bool>(ref value);
                    break;
                case DSharpLiteralType.Short:
                    _short = Unsafe.As<T, short>(ref value);
                    break;
                case DSharpLiteralType.UShort:
                    _ushort = Unsafe.As<T, ushort>(ref value);
                    break;
                case DSharpLiteralType.Char:
                    _char = Unsafe.As<T, char>(ref value);
                    break;
                case DSharpLiteralType.Int:
                    _int = Unsafe.As<T, int>(ref value);
                    break;
                case DSharpLiteralType.UInt:
                    _uint = Unsafe.As<T, uint>(ref value);
                    break;
                case DSharpLiteralType.Long:
                    _long = Unsafe.As<T, long>(ref value);
                    break;
                case DSharpLiteralType.ULong:
                    _ulong = Unsafe.As<T, ulong>(ref value);
                    break;
                case DSharpLiteralType.NInt:
                    _nint = Unsafe.As<T, nint>(ref value);
                    break;
                case DSharpLiteralType.NUInt:
                    _nuint = Unsafe.As<T, nuint>(ref value);
                    break;
                case DSharpLiteralType.Float:
                    _float = Unsafe.As<T, float>(ref value);
                    break;
                case DSharpLiteralType.Double:
                    _double = Unsafe.As<T, double>(ref value);
                    break;
                case DSharpLiteralType.Decimal:
                    _decimal = Unsafe.As<T, decimal>(ref value);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid number type: {type}");
            };
        }
        /// <summary>
        /// Convert current number to specified type
        /// </summary>
        /// <typeparam name="T">Type to converting</typeparam>
        /// <returns>Converted value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T Convert<T>() where T : unmanaged
        {
            return DSharpNumberConverter.Convert<T>(this);
        }
        /// <summary>
        /// Convert current number to specified literal type
        /// </summary>
        /// <param name="type">Literal type for converting</param>
        /// <returns>Converted number</returns>
        /// <exception cref="InvalidOperationException">Invalid number type</exception>
        public readonly DSharpNumber Convert(DSharpLiteralType type)
        {
            var currentType = _type;

            if (currentType == type)
            {
                return this;
            }

            return currentType switch
            {
                DSharpLiteralType.Byte => Create(type, Convert<byte>()),
                DSharpLiteralType.SByte => Create(type, Convert<sbyte>()),
                DSharpLiteralType.Bool => Create(type, Convert<bool>()),
                DSharpLiteralType.Short => Create(type, Convert<short>()),
                DSharpLiteralType.UShort => Create(type, Convert<ushort>()),
                DSharpLiteralType.Char => Create(type, Convert<char>()),
                DSharpLiteralType.Int => Create(type, Convert<int>()),
                DSharpLiteralType.UInt => Create(type, Convert<uint>()),
                DSharpLiteralType.Long => Create(type, Convert<long>()),
                DSharpLiteralType.ULong => Create(type, Convert<ulong>()),
                DSharpLiteralType.NInt => Create(type, Convert<nint>()),
                DSharpLiteralType.NUInt => Create(type, Convert<nuint>()),
                DSharpLiteralType.Float => Create(type, Convert<float>()),
                DSharpLiteralType.Double => Create(type, Convert<double>()),
                DSharpLiteralType.Decimal => Create(type, Convert<float>()),
                _ => throw new InvalidOperationException($"Invalid number type: {currentType}")
            };
        }

        public readonly override int GetHashCode()
        {
            return Convert(DSharpLiteralType.Long).Read<long>().GetHashCode();
        }
        public readonly override bool Equals(object? obj)
        {
            return obj is DSharpNumber other && Equals(other);
        }
        public readonly bool Equals(DSharpNumber other)
        {
            return DSharpNumberMathExecutor.Instance.LogicalEquals(this, other).Read<bool>();
        }
        public readonly override string ToString()
        {
            var type = _type;

            return type switch
            {
                DSharpLiteralType.Byte => _byte.ToString(),
                DSharpLiteralType.SByte => _sbyte.ToString(),
                DSharpLiteralType.Bool => _bool.ToString(),
                DSharpLiteralType.Short => _short.ToString(),
                DSharpLiteralType.UShort => _ushort.ToString(),
                DSharpLiteralType.Char => _char.ToString(),
                DSharpLiteralType.Int => _int.ToString(),
                DSharpLiteralType.UInt => _uint.ToString(),
                DSharpLiteralType.Long => _long.ToString(),
                DSharpLiteralType.ULong => _ulong.ToString(),
                DSharpLiteralType.NInt => _nint.ToString(),
                DSharpLiteralType.NUInt => _nuint.ToString(),
                DSharpLiteralType.Float => _float.ToString(),
                DSharpLiteralType.Double => _double.ToString(),
                DSharpLiteralType.Decimal => _decimal.ToString(),
                _ => "0"
            };
        }

        #endregion

        #region Math operators

        public static DSharpNumber operator ==(DSharpNumber left, DSharpNumber right) => DSharpNumberMathExecutor.Instance.LogicalEquals(left, right);
        public static DSharpNumber operator !=(DSharpNumber left, DSharpNumber right) => DSharpNumberMathExecutor.Instance.LogicalNotEquals(left, right);
        public static DSharpNumber operator +(DSharpNumber left, DSharpNumber right) => DSharpNumberMathExecutor.Instance.Add(left, right);
        public static DSharpNumber operator -(DSharpNumber left, DSharpNumber right) => DSharpNumberMathExecutor.Instance.Subtract(left, right);
        public static DSharpNumber operator *(DSharpNumber left, DSharpNumber right) => DSharpNumberMathExecutor.Instance.Multiply(left, right);
        public static DSharpNumber operator /(DSharpNumber left, DSharpNumber right) => DSharpNumberMathExecutor.Instance.Divide(left, right);
        public static DSharpNumber operator %(DSharpNumber left, DSharpNumber right) => DSharpNumberMathExecutor.Instance.Mod(left, right);
        public static DSharpNumber operator ^(DSharpNumber left, DSharpNumber right) => DSharpNumberMathExecutor.Instance.Xor(left, right);
        public static DSharpNumber operator <<(DSharpNumber left, DSharpNumber right) => DSharpNumberMathExecutor.Instance.ShiftLeft(left, right);
        public static DSharpNumber operator >>(DSharpNumber left, DSharpNumber right) => DSharpNumberMathExecutor.Instance.ShiftRight(left, right);
        public static DSharpNumber operator |(DSharpNumber left, DSharpNumber right) => DSharpNumberMathExecutor.Instance.Or(left, right);
        public static DSharpNumber operator &(DSharpNumber left, DSharpNumber right) => DSharpNumberMathExecutor.Instance.And(left, right);
        public static DSharpNumber operator >(DSharpNumber left, DSharpNumber right) => DSharpNumberMathExecutor.Instance.LogicalGreater(left, right);
        public static DSharpNumber operator >=(DSharpNumber left, DSharpNumber right) => DSharpNumberMathExecutor.Instance.LogicalGreaterOrEquals(left, right);
        public static DSharpNumber operator <(DSharpNumber left, DSharpNumber right) => DSharpNumberMathExecutor.Instance.LogicalLess(left, right);
        public static DSharpNumber operator <=(DSharpNumber left, DSharpNumber right) => DSharpNumberMathExecutor.Instance.LogicalLessOrEquals(left, right);
        public static DSharpNumber operator !(DSharpNumber value) => DSharpNumberMathExecutor.Instance.Not(value);
        public static DSharpNumber operator ++(DSharpNumber value) => DSharpNumberMathExecutor.Instance.Increment(value);
        public static DSharpNumber operator --(DSharpNumber value) => DSharpNumberMathExecutor.Instance.Decrement(value);
        public static DSharpNumber operator ~(DSharpNumber value) => DSharpNumberMathExecutor.Instance.BitInvert(value);

        #endregion

        #region Convert operators

        public static implicit operator DSharpNumber(byte value) => Create(DSharpLiteralType.Byte, value);
        public static implicit operator DSharpNumber(sbyte value) => Create(DSharpLiteralType.SByte, value);
        public static implicit operator DSharpNumber(bool value) => Create(DSharpLiteralType.Bool, value);
        public static implicit operator DSharpNumber(short value) => Create(DSharpLiteralType.Short, value);
        public static implicit operator DSharpNumber(ushort value) => Create(DSharpLiteralType.UShort, value);
        public static implicit operator DSharpNumber(char value) => Create(DSharpLiteralType.Char, value);
        public static implicit operator DSharpNumber(int value) => Create(DSharpLiteralType.Int, value);
        public static implicit operator DSharpNumber(uint value) => Create(DSharpLiteralType.UInt, value);
        public static implicit operator DSharpNumber(long value) => Create(DSharpLiteralType.Long, value);
        public static implicit operator DSharpNumber(ulong value) => Create(DSharpLiteralType.ULong, value);
        public static implicit operator DSharpNumber(nint value) => Create(DSharpLiteralType.NInt, value);
        public static implicit operator DSharpNumber(nuint value) => Create(DSharpLiteralType.NUInt, value);
        public static implicit operator DSharpNumber(float value) => Create(DSharpLiteralType.Float, value);
        public static implicit operator DSharpNumber(double value) => Create(DSharpLiteralType.Double, value);
        public static implicit operator DSharpNumber(decimal value) => Create(DSharpLiteralType.Decimal, value);

        #endregion

        #region Static

        /// <summary>
        /// Number literal values type
        /// </summary>
        public static ReadOnlyDictionary<DSharpLiteralType, int> Sizes
        {
            get
            {
                if (field == null)
                {
                    var literalValues = DSharpLiteralTypeHelper.Values;
                    Dictionary<DSharpLiteralType, int> sizes = new(literalValues.Length);

                    for (int i = 0; i < literalValues.Length; i++)
                    {
                        var literalValue = literalValues[i];

                        if (!DSharpBuildInTypes.TryGetTypeInfo(literalValue, out var info) ||
                            0 >= info.Size)
                        {
                            continue;
                        }

                        sizes.Add(literalValue, info.Size);
                    }

                    field = new(sizes);
                }

                return field;
            }
        }

        /// <summary>
        /// Create D# number
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="type">Type of number</param>
        /// <param name="value">Number value</param>
        /// <returns>D# number</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber Create<T>(DSharpLiteralType type, T value) where T : unmanaged
        {
            DSharpNumber* result = stackalloc DSharpNumber[1];
            result->_type = type;
            Unsafe.WriteUnaligned(result, value);

            return *result;
        }

        /// <summary>
        /// Perform math operation with number
        /// </summary>
        /// <param name="value">Number for performing math operation</param>
        /// <param name="operator">Math unary operator</param>
        /// <returns>Math operation result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber MathOperation(DSharpNumber value, DSharpUnaryOperator @operator)
        {
            return DSharpNumberMathExecutor.Perform(value, @operator);
        }
        /// <summary>
        /// Perform math operation between numbers
        /// </summary>
        /// <param name="left">Left side number for performing math operation</param>
        /// <param name="left">Right side number for performing math operation</param>
        /// <param name="operator">Math binary operator</param>
        /// <returns>Math operation result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpNumber MathOperation(DSharpNumber left, DSharpNumber right, DSharpBinaryOperator @operator)
        {
            return DSharpNumberMathExecutor.Perform(left, right, @operator);
        }

        #endregion

        #region Converters

        internal static void ByteConverter(in DSharpNumber number, void* buffer)
        {
            var type = number._type;

            switch (type)
            {
                case DSharpLiteralType.Byte:
                    Unsafe.WriteUnaligned(buffer, number._byte);
                    break;
                case DSharpLiteralType.SByte:
                    Unsafe.WriteUnaligned(buffer, unchecked((byte)number._sbyte));
                    break;
                case DSharpLiteralType.Bool:
                    Unsafe.WriteUnaligned(buffer, number._bool ? (byte)1 : (byte)0);
                    break;
                case DSharpLiteralType.Short:
                    Unsafe.WriteUnaligned(buffer, unchecked((byte)number._short));
                    break;
                case DSharpLiteralType.UShort:
                    Unsafe.WriteUnaligned(buffer, unchecked((byte)number._ushort));
                    break;
                case DSharpLiteralType.Char:
                    Unsafe.WriteUnaligned(buffer, unchecked((byte)number._char));
                    break;
                case DSharpLiteralType.Int:
                    Unsafe.WriteUnaligned(buffer, unchecked((byte)number._int));
                    break;
                case DSharpLiteralType.UInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((byte)number._uint));
                    break;
                case DSharpLiteralType.Long:
                    Unsafe.WriteUnaligned(buffer, unchecked((byte)number._long));
                    break;
                case DSharpLiteralType.ULong:
                    Unsafe.WriteUnaligned(buffer, unchecked((byte)number._ulong));
                    break;
                case DSharpLiteralType.NInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((byte)number._nint));
                    break;
                case DSharpLiteralType.NUInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((byte)number._nuint));
                    break;
                case DSharpLiteralType.Float:
                    Unsafe.WriteUnaligned(buffer, unchecked((byte)number._float));
                    break;
                case DSharpLiteralType.Double:
                    Unsafe.WriteUnaligned(buffer, unchecked((byte)number._double));
                    break;
                case DSharpLiteralType.Decimal:
                    Unsafe.WriteUnaligned(buffer, unchecked((byte)number._decimal));
                    break;
            }
        }
        internal static void SByteConverter(in DSharpNumber number, void* buffer)
        {
            var type = number._type;

            switch (type)
            {
                case DSharpLiteralType.Byte:
                    Unsafe.WriteUnaligned(buffer, unchecked((sbyte)number._byte));
                    break;
                case DSharpLiteralType.SByte:
                    Unsafe.WriteUnaligned(buffer, number._sbyte);
                    break;
                case DSharpLiteralType.Bool:
                    Unsafe.WriteUnaligned(buffer, number._bool ? (sbyte)1 : (sbyte)0);
                    break;
                case DSharpLiteralType.Short:
                    Unsafe.WriteUnaligned(buffer, unchecked((sbyte)number._short));
                    break;
                case DSharpLiteralType.UShort:
                    Unsafe.WriteUnaligned(buffer, unchecked((sbyte)number._ushort));
                    break;
                case DSharpLiteralType.Char:
                    Unsafe.WriteUnaligned(buffer, unchecked((sbyte)number._char));
                    break;
                case DSharpLiteralType.Int:
                    Unsafe.WriteUnaligned(buffer, unchecked((sbyte)number._int));
                    break;
                case DSharpLiteralType.UInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((sbyte)number._uint));
                    break;
                case DSharpLiteralType.Long:
                    Unsafe.WriteUnaligned(buffer, unchecked((sbyte)number._long));
                    break;
                case DSharpLiteralType.ULong:
                    Unsafe.WriteUnaligned(buffer, unchecked((sbyte)number._ulong));
                    break;
                case DSharpLiteralType.NInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((sbyte)number._nint));
                    break;
                case DSharpLiteralType.NUInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((sbyte)number._nuint));
                    break;
                case DSharpLiteralType.Float:
                    Unsafe.WriteUnaligned(buffer, unchecked((sbyte)number._float));
                    break;
                case DSharpLiteralType.Double:
                    Unsafe.WriteUnaligned(buffer, unchecked((sbyte)number._double));
                    break;
                case DSharpLiteralType.Decimal:
                    Unsafe.WriteUnaligned(buffer, unchecked((sbyte)number._decimal));
                    break;
            }
        }
        internal static void BoolConverter(in DSharpNumber number, void* buffer)
        {
            var type = number._type;

            switch (type)
            {
                case DSharpLiteralType.Byte:
                    Unsafe.WriteUnaligned(buffer, number._bool ? (byte)1 : (byte)0);
                    break;
                case DSharpLiteralType.SByte:
                    Unsafe.WriteUnaligned(buffer, number._bool ? (sbyte)1 : (sbyte)0);
                    break;
                case DSharpLiteralType.Bool:
                    Unsafe.WriteUnaligned(buffer, number._bool);
                    break;
                case DSharpLiteralType.Short:
                    Unsafe.WriteUnaligned(buffer, number._bool ? (short)1 : (short)0);
                    break;
                case DSharpLiteralType.UShort:
                    Unsafe.WriteUnaligned(buffer, number._bool ? (ushort)1 : (ushort)0);
                    break;
                case DSharpLiteralType.Char:
                    Unsafe.WriteUnaligned(buffer, number._bool ? (char)1 : '\0');
                    break;
                case DSharpLiteralType.Int:
                    Unsafe.WriteUnaligned(buffer, number._bool ? 1 : 0);
                    break;
                case DSharpLiteralType.UInt:
                    Unsafe.WriteUnaligned(buffer, number._bool ? 1u : 0u);
                    break;
                case DSharpLiteralType.Long:
                    Unsafe.WriteUnaligned(buffer, number._bool ? 1L : 0L);
                    break;
                case DSharpLiteralType.ULong:
                    Unsafe.WriteUnaligned(buffer, number._bool ? 1UL : 0UL);
                    break;
                case DSharpLiteralType.NInt:
                    Unsafe.WriteUnaligned(buffer, number._bool ? (IntPtr)1 : IntPtr.Zero);
                    break;
                case DSharpLiteralType.NUInt:
                    Unsafe.WriteUnaligned(buffer, number._bool ? (UIntPtr)1 : UIntPtr.Zero);
                    break;
                case DSharpLiteralType.Float:
                    Unsafe.WriteUnaligned(buffer, number._bool ? 1f : 0f);
                    break;
                case DSharpLiteralType.Double:
                    Unsafe.WriteUnaligned(buffer, number._bool ? 1d : 0d);
                    break;
                case DSharpLiteralType.Decimal:
                    Unsafe.WriteUnaligned(buffer, number._bool ? decimal.One : decimal.Zero);
                    break;
            }
        }
        internal static void ShortConverter(in DSharpNumber number, void* buffer)
        {
            var type = number._type;

            switch (type)
            {
                case DSharpLiteralType.Byte:
                    Unsafe.WriteUnaligned(buffer, unchecked((short)number._byte));
                    break;
                case DSharpLiteralType.SByte:
                    Unsafe.WriteUnaligned(buffer, unchecked((short)number._sbyte));
                    break;
                case DSharpLiteralType.Bool:
                    Unsafe.WriteUnaligned(buffer, number._bool ? (short)1 : (short)0);
                    break;
                case DSharpLiteralType.Short:
                    Unsafe.WriteUnaligned(buffer, number._short);
                    break;
                case DSharpLiteralType.UShort:
                    Unsafe.WriteUnaligned(buffer, unchecked((short)number._ushort));
                    break;
                case DSharpLiteralType.Char:
                    Unsafe.WriteUnaligned(buffer, unchecked((short)number._char));
                    break;
                case DSharpLiteralType.Int:
                    Unsafe.WriteUnaligned(buffer, unchecked((short)number._int));
                    break;
                case DSharpLiteralType.UInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((short)number._uint));
                    break;
                case DSharpLiteralType.Long:
                    Unsafe.WriteUnaligned(buffer, unchecked((short)number._long));
                    break;
                case DSharpLiteralType.ULong:
                    Unsafe.WriteUnaligned(buffer, unchecked((short)number._ulong));
                    break;
                case DSharpLiteralType.NInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((short)number._nint));
                    break;
                case DSharpLiteralType.NUInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((short)number._nuint));
                    break;
                case DSharpLiteralType.Float:
                    Unsafe.WriteUnaligned(buffer, unchecked((short)number._float));
                    break;
                case DSharpLiteralType.Double:
                    Unsafe.WriteUnaligned(buffer, unchecked((short)number._double));
                    break;
                case DSharpLiteralType.Decimal:
                    Unsafe.WriteUnaligned(buffer, unchecked((short)number._decimal));
                    break;
            }
        }
        internal static void UShortConverter(in DSharpNumber number, void* buffer)
        {
            var type = number._type;

            switch (type)
            {
                case DSharpLiteralType.Byte:
                    Unsafe.WriteUnaligned(buffer, unchecked((ushort)number._byte));
                    break;
                case DSharpLiteralType.SByte:
                    Unsafe.WriteUnaligned(buffer, unchecked((ushort)number._sbyte));
                    break;
                case DSharpLiteralType.Bool:
                    Unsafe.WriteUnaligned(buffer, number._bool ? (ushort)1 : (ushort)0);
                    break;
                case DSharpLiteralType.Short:
                    Unsafe.WriteUnaligned(buffer, unchecked((ushort)number._short));
                    break;
                case DSharpLiteralType.UShort:
                    Unsafe.WriteUnaligned(buffer, number._ushort);
                    break;
                case DSharpLiteralType.Char:
                    Unsafe.WriteUnaligned(buffer, unchecked((ushort)number._char));
                    break;
                case DSharpLiteralType.Int:
                    Unsafe.WriteUnaligned(buffer, unchecked((ushort)number._int));
                    break;
                case DSharpLiteralType.UInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((ushort)number._uint));
                    break;
                case DSharpLiteralType.Long:
                    Unsafe.WriteUnaligned(buffer, unchecked((ushort)number._long));
                    break;
                case DSharpLiteralType.ULong:
                    Unsafe.WriteUnaligned(buffer, unchecked((ushort)number._ulong));
                    break;
                case DSharpLiteralType.NInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((ushort)number._nint));
                    break;
                case DSharpLiteralType.NUInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((ushort)number._nuint));
                    break;
                case DSharpLiteralType.Float:
                    Unsafe.WriteUnaligned(buffer, unchecked((ushort)number._float));
                    break;
                case DSharpLiteralType.Double:
                    Unsafe.WriteUnaligned(buffer, unchecked((ushort)number._double));
                    break;
                case DSharpLiteralType.Decimal:
                    Unsafe.WriteUnaligned(buffer, unchecked((ushort)number._decimal));
                    break;
            }
        }
        internal static void CharConverter(in DSharpNumber number, void* buffer)
        {
            var type = number._type;

            switch (type)
            {
                case DSharpLiteralType.Byte:
                    Unsafe.WriteUnaligned(buffer, unchecked((char)number._byte));
                    break;
                case DSharpLiteralType.SByte:
                    Unsafe.WriteUnaligned(buffer, unchecked((char)number._sbyte));
                    break;
                case DSharpLiteralType.Bool:
                    Unsafe.WriteUnaligned(buffer, number._bool ? (char)1 : '\0');
                    break;
                case DSharpLiteralType.Short:
                    Unsafe.WriteUnaligned(buffer, unchecked((char)number._short));
                    break;
                case DSharpLiteralType.UShort:
                    Unsafe.WriteUnaligned(buffer, unchecked((char)number._ushort));
                    break;
                case DSharpLiteralType.Char:
                    Unsafe.WriteUnaligned(buffer, number._char);
                    break;
                case DSharpLiteralType.Int:
                    Unsafe.WriteUnaligned(buffer, unchecked((char)number._int));
                    break;
                case DSharpLiteralType.UInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((char)number._uint));
                    break;
                case DSharpLiteralType.Long:
                    Unsafe.WriteUnaligned(buffer, unchecked((char)number._long));
                    break;
                case DSharpLiteralType.ULong:
                    Unsafe.WriteUnaligned(buffer, unchecked((char)number._ulong));
                    break;
                case DSharpLiteralType.NInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((char)number._nint));
                    break;
                case DSharpLiteralType.NUInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((char)number._nuint));
                    break;
                case DSharpLiteralType.Float:
                    Unsafe.WriteUnaligned(buffer, unchecked((char)number._float));
                    break;
                case DSharpLiteralType.Double:
                    Unsafe.WriteUnaligned(buffer, unchecked((char)number._double));
                    break;
                case DSharpLiteralType.Decimal:
                    Unsafe.WriteUnaligned(buffer, unchecked((char)number._decimal));
                    break;
            }
        }
        internal static void IntConverter(in DSharpNumber number, void* buffer)
        {
            var type = number._type;

            switch (type)
            {
                case DSharpLiteralType.Byte:
                    Unsafe.WriteUnaligned(buffer, unchecked((int)number._byte));
                    break;
                case DSharpLiteralType.SByte:
                    Unsafe.WriteUnaligned(buffer, unchecked((int)number._sbyte));
                    break;
                case DSharpLiteralType.Bool:
                    Unsafe.WriteUnaligned(buffer, number._bool ? 1 : 0);
                    break;
                case DSharpLiteralType.Short:
                    Unsafe.WriteUnaligned(buffer, unchecked((int)number._short));
                    break;
                case DSharpLiteralType.UShort:
                    Unsafe.WriteUnaligned(buffer, unchecked((int)number._ushort));
                    break;
                case DSharpLiteralType.Char:
                    Unsafe.WriteUnaligned(buffer, unchecked((int)number._char));
                    break;
                case DSharpLiteralType.Int:
                    Unsafe.WriteUnaligned(buffer, number._int);
                    break;
                case DSharpLiteralType.UInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((int)number._uint));
                    break;
                case DSharpLiteralType.Long:
                    Unsafe.WriteUnaligned(buffer, unchecked((int)number._long));
                    break;
                case DSharpLiteralType.ULong:
                    Unsafe.WriteUnaligned(buffer, unchecked((int)number._ulong));
                    break;
                case DSharpLiteralType.NInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((int)number._nint));
                    break;
                case DSharpLiteralType.NUInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((int)number._nuint));
                    break;
                case DSharpLiteralType.Float:
                    Unsafe.WriteUnaligned(buffer, unchecked((int)number._float));
                    break;
                case DSharpLiteralType.Double:
                    Unsafe.WriteUnaligned(buffer, unchecked((int)number._double));
                    break;
                case DSharpLiteralType.Decimal:
                    Unsafe.WriteUnaligned(buffer, unchecked((int)number._decimal));
                    break;
            }
        }
        internal static void UIntConverter(in DSharpNumber number, void* buffer)
        {
            var type = number._type;

            switch (type)
            {
                case DSharpLiteralType.Byte:
                    Unsafe.WriteUnaligned(buffer, unchecked((uint)number._byte));
                    break;
                case DSharpLiteralType.SByte:
                    Unsafe.WriteUnaligned(buffer, unchecked((uint)number._sbyte));
                    break;
                case DSharpLiteralType.Bool:
                    Unsafe.WriteUnaligned(buffer, number._bool ? 1u : 0u);
                    break;
                case DSharpLiteralType.Short:
                    Unsafe.WriteUnaligned(buffer, unchecked((uint)number._short));
                    break;
                case DSharpLiteralType.UShort:
                    Unsafe.WriteUnaligned(buffer, unchecked((uint)number._ushort));
                    break;
                case DSharpLiteralType.Char:
                    Unsafe.WriteUnaligned(buffer, unchecked((uint)number._char));
                    break;
                case DSharpLiteralType.Int:
                    Unsafe.WriteUnaligned(buffer, unchecked((uint)number._int));
                    break;
                case DSharpLiteralType.UInt:
                    Unsafe.WriteUnaligned(buffer, number._uint);
                    break;
                case DSharpLiteralType.Long:
                    Unsafe.WriteUnaligned(buffer, unchecked((uint)number._long));
                    break;
                case DSharpLiteralType.ULong:
                    Unsafe.WriteUnaligned(buffer, unchecked((uint)number._ulong));
                    break;
                case DSharpLiteralType.NInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((uint)number._nint));
                    break;
                case DSharpLiteralType.NUInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((uint)number._nuint));
                    break;
                case DSharpLiteralType.Float:
                    Unsafe.WriteUnaligned(buffer, unchecked((uint)number._float));
                    break;
                case DSharpLiteralType.Double:
                    Unsafe.WriteUnaligned(buffer, unchecked((uint)number._double));
                    break;
                case DSharpLiteralType.Decimal:
                    Unsafe.WriteUnaligned(buffer, unchecked((uint)number._decimal));
                    break;
            }
        }
        internal static void LongConverter(in DSharpNumber number, void* buffer)
        {
            var type = number._type;

            switch (type)
            {
                case DSharpLiteralType.Byte:
                    Unsafe.WriteUnaligned(buffer, unchecked((long)number._byte));
                    break;
                case DSharpLiteralType.SByte:
                    Unsafe.WriteUnaligned(buffer, unchecked((long)number._sbyte));
                    break;
                case DSharpLiteralType.Bool:
                    Unsafe.WriteUnaligned(buffer, number._bool ? 1L : 0L);
                    break;
                case DSharpLiteralType.Short:
                    Unsafe.WriteUnaligned(buffer, unchecked((long)number._short));
                    break;
                case DSharpLiteralType.UShort:
                    Unsafe.WriteUnaligned(buffer, unchecked((long)number._ushort));
                    break;
                case DSharpLiteralType.Char:
                    Unsafe.WriteUnaligned(buffer, unchecked((long)number._char));
                    break;
                case DSharpLiteralType.Int:
                    Unsafe.WriteUnaligned(buffer, unchecked((long)number._int));
                    break;
                case DSharpLiteralType.UInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((long)number._uint));
                    break;
                case DSharpLiteralType.Long:
                    Unsafe.WriteUnaligned(buffer, number._long);
                    break;
                case DSharpLiteralType.ULong:
                    Unsafe.WriteUnaligned(buffer, unchecked((long)number._ulong));
                    break;
                case DSharpLiteralType.NInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((long)number._nint));
                    break;
                case DSharpLiteralType.NUInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((long)number._nuint));
                    break;
                case DSharpLiteralType.Float:
                    Unsafe.WriteUnaligned(buffer, unchecked((long)number._float));
                    break;
                case DSharpLiteralType.Double:
                    Unsafe.WriteUnaligned(buffer, unchecked((long)number._double));
                    break;
                case DSharpLiteralType.Decimal:
                    Unsafe.WriteUnaligned(buffer, unchecked((long)number._decimal));
                    break;
            }
        }
        internal static void ULongConverter(in DSharpNumber number, void* buffer)
        {
            var type = number._type;

            switch (type)
            {
                case DSharpLiteralType.Byte:
                    Unsafe.WriteUnaligned(buffer, unchecked((ulong)number._byte));
                    break;
                case DSharpLiteralType.SByte:
                    Unsafe.WriteUnaligned(buffer, unchecked((ulong)number._sbyte));
                    break;
                case DSharpLiteralType.Bool:
                    Unsafe.WriteUnaligned(buffer, number._bool ? 1UL : 0UL);
                    break;
                case DSharpLiteralType.Short:
                    Unsafe.WriteUnaligned(buffer, unchecked((ulong)number._short));
                    break;
                case DSharpLiteralType.UShort:
                    Unsafe.WriteUnaligned(buffer, unchecked((ulong)number._ushort));
                    break;
                case DSharpLiteralType.Char:
                    Unsafe.WriteUnaligned(buffer, unchecked((ulong)number._char));
                    break;
                case DSharpLiteralType.Int:
                    Unsafe.WriteUnaligned(buffer, unchecked((ulong)number._int));
                    break;
                case DSharpLiteralType.UInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((ulong)number._uint));
                    break;
                case DSharpLiteralType.Long:
                    Unsafe.WriteUnaligned(buffer, unchecked((ulong)number._long));
                    break;
                case DSharpLiteralType.ULong:
                    Unsafe.WriteUnaligned(buffer, number._ulong);
                    break;
                case DSharpLiteralType.NInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((ulong)number._nint));
                    break;
                case DSharpLiteralType.NUInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((ulong)number._nuint));
                    break;
                case DSharpLiteralType.Float:
                    Unsafe.WriteUnaligned(buffer, unchecked((ulong)number._float));
                    break;
                case DSharpLiteralType.Double:
                    Unsafe.WriteUnaligned(buffer, unchecked((ulong)number._double));
                    break;
                case DSharpLiteralType.Decimal:
                    Unsafe.WriteUnaligned(buffer, unchecked((ulong)number._decimal));
                    break;
            }
        }
        internal static void NIntConverter(in DSharpNumber number, void* buffer)
        {
            var type = number._type;

            switch (type)
            {
                case DSharpLiteralType.Byte:
                    Unsafe.WriteUnaligned(buffer, unchecked((nint)number._byte));
                    break;
                case DSharpLiteralType.SByte:
                    Unsafe.WriteUnaligned(buffer, unchecked((nint)number._sbyte));
                    break;
                case DSharpLiteralType.Bool:
                    Unsafe.WriteUnaligned(buffer, number._bool ? (IntPtr)1 : IntPtr.Zero);
                    break;
                case DSharpLiteralType.Short:
                    Unsafe.WriteUnaligned(buffer, unchecked((nint)number._short));
                    break;
                case DSharpLiteralType.UShort:
                    Unsafe.WriteUnaligned(buffer, unchecked((nint)number._ushort));
                    break;
                case DSharpLiteralType.Char:
                    Unsafe.WriteUnaligned(buffer, unchecked((nint)number._char));
                    break;
                case DSharpLiteralType.Int:
                    Unsafe.WriteUnaligned(buffer, unchecked((nint)number._int));
                    break;
                case DSharpLiteralType.UInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((nint)number._uint));
                    break;
                case DSharpLiteralType.Long:
                    Unsafe.WriteUnaligned(buffer, unchecked((nint)number._long));
                    break;
                case DSharpLiteralType.ULong:
                    Unsafe.WriteUnaligned(buffer, unchecked((nint)number._ulong));
                    break;
                case DSharpLiteralType.NInt:
                    Unsafe.WriteUnaligned(buffer, number._nint);
                    break;
                case DSharpLiteralType.NUInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((nint)number._nuint));
                    break;
                case DSharpLiteralType.Float:
                    Unsafe.WriteUnaligned(buffer, unchecked((nint)number._float));
                    break;
                case DSharpLiteralType.Double:
                    Unsafe.WriteUnaligned(buffer, unchecked((nint)number._double));
                    break;
                case DSharpLiteralType.Decimal:
                    Unsafe.WriteUnaligned(buffer, unchecked((nint)number._decimal));
                    break;
            }
        }
        internal static void NUIntConverter(in DSharpNumber number, void* buffer)
        {
            var type = number._type;

            switch (type)
            {
                case DSharpLiteralType.Byte:
                    Unsafe.WriteUnaligned(buffer, unchecked((nuint)number._byte));
                    break;
                case DSharpLiteralType.SByte:
                    Unsafe.WriteUnaligned(buffer, unchecked((nuint)number._sbyte));
                    break;
                case DSharpLiteralType.Bool:
                    Unsafe.WriteUnaligned(buffer, number._bool ? (UIntPtr)1 : UIntPtr.Zero);
                    break;
                case DSharpLiteralType.Short:
                    Unsafe.WriteUnaligned(buffer, unchecked((nuint)number._short));
                    break;
                case DSharpLiteralType.UShort:
                    Unsafe.WriteUnaligned(buffer, unchecked((nuint)number._ushort));
                    break;
                case DSharpLiteralType.Char:
                    Unsafe.WriteUnaligned(buffer, unchecked((nuint)number._char));
                    break;
                case DSharpLiteralType.Int:
                    Unsafe.WriteUnaligned(buffer, unchecked((nuint)number._int));
                    break;
                case DSharpLiteralType.UInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((nuint)number._uint));
                    break;
                case DSharpLiteralType.Long:
                    Unsafe.WriteUnaligned(buffer, unchecked((nuint)number._long));
                    break;
                case DSharpLiteralType.ULong:
                    Unsafe.WriteUnaligned(buffer, unchecked((nuint)number._ulong));
                    break;
                case DSharpLiteralType.NInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((nuint)number._nint));
                    break;
                case DSharpLiteralType.NUInt:
                    Unsafe.WriteUnaligned(buffer, number._nuint);
                    break;
                case DSharpLiteralType.Float:
                    Unsafe.WriteUnaligned(buffer, unchecked((nuint)number._float));
                    break;
                case DSharpLiteralType.Double:
                    Unsafe.WriteUnaligned(buffer, unchecked((nuint)number._double));
                    break;
                case DSharpLiteralType.Decimal:
                    Unsafe.WriteUnaligned(buffer, unchecked((nuint)number._decimal));
                    break;
            }
        }
        internal static void FloatConverter(in DSharpNumber number, void* buffer)
        {
            var type = number._type;

            switch (type)
            {
                case DSharpLiteralType.Byte:
                    Unsafe.WriteUnaligned(buffer, unchecked((float)number._byte));
                    break;
                case DSharpLiteralType.SByte:
                    Unsafe.WriteUnaligned(buffer, unchecked((float)number._sbyte));
                    break;
                case DSharpLiteralType.Bool:
                    Unsafe.WriteUnaligned(buffer, number._bool ? 1f : 0f);
                    break;
                case DSharpLiteralType.Short:
                    Unsafe.WriteUnaligned(buffer, unchecked((float)number._short));
                    break;
                case DSharpLiteralType.UShort:
                    Unsafe.WriteUnaligned(buffer, unchecked((float)number._ushort));
                    break;
                case DSharpLiteralType.Char:
                    Unsafe.WriteUnaligned(buffer, unchecked((float)number._char));
                    break;
                case DSharpLiteralType.Int:
                    Unsafe.WriteUnaligned(buffer, unchecked((float)number._int));
                    break;
                case DSharpLiteralType.UInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((float)number._uint));
                    break;
                case DSharpLiteralType.Long:
                    Unsafe.WriteUnaligned(buffer, unchecked((float)number._long));
                    break;
                case DSharpLiteralType.ULong:
                    Unsafe.WriteUnaligned(buffer, unchecked((float)number._ulong));
                    break;
                case DSharpLiteralType.NInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((float)number._nint));
                    break;
                case DSharpLiteralType.NUInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((float)number._nuint));
                    break;
                case DSharpLiteralType.Float:
                    Unsafe.WriteUnaligned(buffer, number._float);
                    break;
                case DSharpLiteralType.Double:
                    Unsafe.WriteUnaligned(buffer, unchecked((float)number._double));
                    break;
                case DSharpLiteralType.Decimal:
                    Unsafe.WriteUnaligned(buffer, unchecked((float)number._decimal));
                    break;
            }
        }
        internal static void DoubleConverter(in DSharpNumber number, void* buffer)
        {
            var type = number._type;

            switch (type)
            {
                case DSharpLiteralType.Byte:
                    Unsafe.WriteUnaligned(buffer, unchecked((double)number._byte));
                    break;
                case DSharpLiteralType.SByte:
                    Unsafe.WriteUnaligned(buffer, unchecked((double)number._sbyte));
                    break;
                case DSharpLiteralType.Bool:
                    Unsafe.WriteUnaligned(buffer, number._bool ? 1d : 0d);
                    break;
                case DSharpLiteralType.Short:
                    Unsafe.WriteUnaligned(buffer, unchecked((double)number._short));
                    break;
                case DSharpLiteralType.UShort:
                    Unsafe.WriteUnaligned(buffer, unchecked((double)number._ushort));
                    break;
                case DSharpLiteralType.Char:
                    Unsafe.WriteUnaligned(buffer, unchecked((double)number._char));
                    break;
                case DSharpLiteralType.Int:
                    Unsafe.WriteUnaligned(buffer, unchecked((double)number._int));
                    break;
                case DSharpLiteralType.UInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((double)number._uint));
                    break;
                case DSharpLiteralType.Long:
                    Unsafe.WriteUnaligned(buffer, unchecked((double)number._long));
                    break;
                case DSharpLiteralType.ULong:
                    Unsafe.WriteUnaligned(buffer, unchecked((double)number._ulong));
                    break;
                case DSharpLiteralType.NInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((double)number._nint));
                    break;
                case DSharpLiteralType.NUInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((double)number._nuint));
                    break;
                case DSharpLiteralType.Float:
                    Unsafe.WriteUnaligned(buffer, unchecked((double)number._float));
                    break;
                case DSharpLiteralType.Double:
                    Unsafe.WriteUnaligned(buffer, number._double);
                    break;
                case DSharpLiteralType.Decimal:
                    Unsafe.WriteUnaligned(buffer, unchecked((double)number._decimal));
                    break;
            }
        }
        internal static void DecimalConverter(in DSharpNumber number, void* buffer)
        {
            var type = number._type;

            switch (type)
            {
                case DSharpLiteralType.Byte:
                    Unsafe.WriteUnaligned(buffer, unchecked((decimal)number._byte));
                    break;
                case DSharpLiteralType.SByte:
                    Unsafe.WriteUnaligned(buffer, unchecked((decimal)number._sbyte));
                    break;
                case DSharpLiteralType.Bool:
                    Unsafe.WriteUnaligned(buffer, number._bool ? decimal.One : decimal.Zero);
                    break;
                case DSharpLiteralType.Short:
                    Unsafe.WriteUnaligned(buffer, unchecked((decimal)number._short));
                    break;
                case DSharpLiteralType.UShort:
                    Unsafe.WriteUnaligned(buffer, unchecked((decimal)number._ushort));
                    break;
                case DSharpLiteralType.Char:
                    Unsafe.WriteUnaligned(buffer, unchecked((decimal)number._char));
                    break;
                case DSharpLiteralType.Int:
                    Unsafe.WriteUnaligned(buffer, unchecked((decimal)number._int));
                    break;
                case DSharpLiteralType.UInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((decimal)number._uint));
                    break;
                case DSharpLiteralType.Long:
                    Unsafe.WriteUnaligned(buffer, unchecked((decimal)number._long));
                    break;
                case DSharpLiteralType.ULong:
                    Unsafe.WriteUnaligned(buffer, unchecked((decimal)number._ulong));
                    break;
                case DSharpLiteralType.NInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((decimal)number._nint));
                    break;
                case DSharpLiteralType.NUInt:
                    Unsafe.WriteUnaligned(buffer, unchecked((decimal)number._nuint));
                    break;
                case DSharpLiteralType.Float:
                    Unsafe.WriteUnaligned(buffer, unchecked((decimal)number._float));
                    break;
                case DSharpLiteralType.Double:
                    Unsafe.WriteUnaligned(buffer, unchecked((decimal)number._double));
                    break;
                case DSharpLiteralType.Decimal:
                    Unsafe.WriteUnaligned(buffer, number._decimal);
                    break;
            }
        }

        #endregion
    }
}

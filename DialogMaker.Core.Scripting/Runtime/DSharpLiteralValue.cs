using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Globalization;
using System.Numerics;
using System.Reflection;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Literal value of D#
    /// </summary>
    public struct DSharpLiteralValue : IEquatable<DSharpLiteralValue>
    {
        /// <summary>
        /// Create literal value based on string
        /// </summary>
        /// <param name="text">String as value literal value</param>
        public DSharpLiteralValue(string? text)
        {
            _stringValue = text;
            Type = text != null ? DSharpLiteralType.String : DSharpLiteralType.Null;
        }
        /// <summary>
        /// Create literal value based on number
        /// </summary>
        /// <param name="text">Number as value literal value</param>
        public DSharpLiteralValue(int number)
        {
            _numberValue = number;
            Type = DSharpLiteralType.Int;
        }
        /// <summary>
        /// Create literal value based on number
        /// </summary>
        /// <param name="text">Number as value literal value</param>
        public DSharpLiteralValue(uint number)
        {
            _numberValue = number;
            Type = DSharpLiteralType.UInt;
        }
        /// <summary>
        /// Create literal value based on number
        /// </summary>
        /// <param name="text">Number as value literal value</param>
        public DSharpLiteralValue(long number)
        {
            _numberValue = number;
            Type = DSharpLiteralType.Long;
        }
        /// <summary>
        /// Create literal value based on number
        /// </summary>
        /// <param name="text">Number as value literal value</param>
        public DSharpLiteralValue(ulong number)
        {
            _numberValue = number;
            Type = DSharpLiteralType.ULong;
        }
        /// <summary>
        /// Create literal value based on number
        /// </summary>
        /// <param name="text">Number as value literal value</param>
        public DSharpLiteralValue(short number)
        {
            _numberValue = number;
            Type = DSharpLiteralType.Short;
        }
        /// <summary>
        /// Create literal value based on number
        /// </summary>
        /// <param name="text">Number as value literal value</param>
        public DSharpLiteralValue(ushort number)
        {
            _numberValue = number;
            Type = DSharpLiteralType.UShort;
        }
        /// <summary>
        /// Create literal value based on number
        /// </summary>
        /// <param name="text">Number as value literal value</param>
        public DSharpLiteralValue(byte number)
        {
            _numberValue = number;
            Type = DSharpLiteralType.Byte;
        }
        /// <summary>
        /// Create literal value based on number
        /// </summary>
        /// <param name="text">Number as value literal value</param>
        public DSharpLiteralValue(sbyte number)
        {
            _numberValue = number;
            Type = DSharpLiteralType.SByte;
        }
        /// <summary>
        /// Create literal value based on number
        /// </summary>
        /// <param name="text">Number as value literal value</param>
        public DSharpLiteralValue(nint number)
        {
            _numberValue = number;
            Type = DSharpLiteralType.NInt;
        }
        /// <summary>
        /// Create literal value based on number
        /// </summary>
        /// <param name="text">Number as value literal value</param>
        public DSharpLiteralValue(nuint number)
        {
            _numberValue = number;
            Type = DSharpLiteralType.NUInt;
        }
        /// <summary>
        /// Create literal value based on number
        /// </summary>
        /// <param name="text">Number as value literal value</param>
        public DSharpLiteralValue(float number)
        {
            _numberValue = number;
            Type = DSharpLiteralType.Float;
        }
        /// <summary>
        /// Create literal value based on number
        /// </summary>
        /// <param name="text">Number as value literal value</param>
        public DSharpLiteralValue(double number)
        {
            _numberValue = number;
            Type = DSharpLiteralType.Double;
        }
        /// <summary>
        /// Create literal value based on number
        /// </summary>
        /// <param name="text">Number as value literal value</param>
        public DSharpLiteralValue(decimal number)
        {
            _numberValue = number;
            Type = DSharpLiteralType.Decimal;
        }
        /// <summary>
        /// Create literal value based on boolean
        /// </summary>
        /// <param name="text">Boolean as value literal value</param>
        public DSharpLiteralValue(bool boolean)
        {
            _boolValue = boolean;
            Type = DSharpLiteralType.Bool;
        }
        /// <summary>
        /// Create literal value based on char
        /// </summary>
        /// <param name="text">Char as value literal value</param>
        public DSharpLiteralValue(char character)
        {
            _charValue = character;
            Type = DSharpLiteralType.Char;
        }

        /// <summary>
        /// Type of literal value
        /// </summary>
        public DSharpLiteralType Type { get; private set; } = DSharpLiteralType.Null;
        /// <summary>
        /// Is literal value empty
        /// </summary>
        public readonly bool IsNull => _stringValue == null && _numberValue == null && _boolValue == null && _charValue == null;
        /// <summary>
        /// Is literal value string
        /// </summary>
        public readonly bool IsString => _stringValue != null;
        /// <summary>
        /// Is literal value number
        /// </summary>
        public readonly bool IsNumber => _numberValue != null;
        /// <summary>
        /// Is literal value boolean
        /// </summary>
        public readonly bool IsBool => _boolValue != null;
        /// <summary>
        /// Is literal value char
        /// </summary>
        public readonly bool IsChar => _charValue != null;

        private readonly string? _stringValue;
        private readonly bool? _boolValue;
        private readonly char? _charValue;
        private object? _numberValue;

        #region Управление

        /// <summary>
        /// Get string value of this literal value
        /// </summary>
        /// <returns>String value</returns>
        /// <exception cref="InvalidCastException">Literal value is not a string</exception>
        public readonly string AsString() => _stringValue ?? throw new InvalidCastException("Literal value is not a string");
        /// <summary>
        /// Get number value of this literal value
        /// </summary>
        /// <returns>Number value</returns>
        /// <exception cref="InvalidCastException">Literal value is not a number</exception>
        public readonly object AsNumber() => _numberValue ?? throw new InvalidCastException("Literal value is not a number");
        /// <summary>
        /// Get number value of this literal value
        /// </summary>
        /// <returns>Number value</returns>
        /// <exception cref="InvalidCastException">Literal value is not a number</exception>
        public readonly T AsNumber<T>() where T : struct
        {
            if (_numberValue == null)
            {
                throw new InvalidCastException("Literal value is not a number");
            }

            return (T)Convert.ChangeType(_numberValue, typeof(T));
        }
        /// <summary>
        /// Get boolean value of this literal value
        /// </summary>
        /// <returns>Boolean value</returns>
        /// <exception cref="InvalidCastException">Literal value is not a boolean</exception>
        public readonly bool AsBool() => _boolValue ?? throw new InvalidCastException("Literal value is not a boolean");
        /// <summary>
        /// Get boolean value of this literal value
        /// </summary>
        /// <returns>Boolean value</returns>
        /// <exception cref="InvalidCastException">Literal value is not a boolean</exception>
        public readonly char AsChar() => _charValue ?? throw new InvalidCastException("Literal value is not a char");

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public readonly override string ToString()
        {
            return _stringValue ?? _numberValue?.ToString() ?? _charValue?.ToString() ?? _boolValue?.ToString() ?? NullString;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(_stringValue, _numberValue, _charValue, _boolValue);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public readonly override bool Equals(object obj)
        {
            return obj is DSharpLiteralValue other &&
                   Equals(other);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="other"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public readonly bool Equals(DSharpLiteralValue other)
        {
            return ToString() == other.ToString();
            //return _stringValue == other._stringValue &&
            //       _numberValue == other._numberValue &&
            //       _boolValue == other._boolValue &&
            //       _charValue == other._charValue;
        }

        #endregion

        #region Операторы

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="l"><inheritdoc/></param>
        /// <param name="r"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator ==(DSharpLiteralValue l, DSharpLiteralValue r) => l.Equals(r);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="l"><inheritdoc/></param>
        /// <param name="r"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator !=(DSharpLiteralValue l, DSharpLiteralValue r) => !l.Equals(r);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="text"><inheritdoc/></param>
        public static implicit operator DSharpLiteralValue(string? text) => new(text);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="number"><inheritdoc/></param>
        public static implicit operator DSharpLiteralValue(double number) => new(number);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="number"><inheritdoc/></param>
        public static implicit operator DSharpLiteralValue(int number) => new(number);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="number"><inheritdoc/></param>
        public static implicit operator DSharpLiteralValue(uint number) => new(number);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="number"><inheritdoc/></param>
        public static implicit operator DSharpLiteralValue(long number) => new(number);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="number"><inheritdoc/></param>
        public static implicit operator DSharpLiteralValue(ulong number) => new(number);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="number"><inheritdoc/></param>
        public static implicit operator DSharpLiteralValue(short number) => new(number);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="number"><inheritdoc/></param>
        public static implicit operator DSharpLiteralValue(ushort number) => new(number);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="number"><inheritdoc/></param>
        public static implicit operator DSharpLiteralValue(byte number) => new(number);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="number"><inheritdoc/></param>
        public static implicit operator DSharpLiteralValue(sbyte number) => new(number);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="number"><inheritdoc/></param>
        public static implicit operator DSharpLiteralValue(nint number) => new(number);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="number"><inheritdoc/></param>
        public static implicit operator DSharpLiteralValue(nuint number) => new(number);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="number"><inheritdoc/></param>
        public static implicit operator DSharpLiteralValue(float number) => new(number);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="number"><inheritdoc/></param>
        public static implicit operator DSharpLiteralValue(decimal number) => new(number);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="boolean"><inheritdoc/></param>
        public static implicit operator DSharpLiteralValue(bool boolean) => new(boolean);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="character"><inheritdoc/></param>
        public static implicit operator DSharpLiteralValue(char character) => new(character);

        #endregion

        #region Константы

        /// <summary>
        /// Null text
        /// </summary>
        public const string NullString = "null";

        #endregion

        #region Статика

        /// <summary>
        /// Null literal value
        /// </summary>
        public static readonly DSharpLiteralValue Null = new();

        private static readonly Assembly _standardAssembly = typeof(int).Assembly;
        private static readonly DSharpLiteralType[] _floatingPointTypes =
        [
            DSharpLiteralType.Decimal,
            DSharpLiteralType.Double,
            DSharpLiteralType.Float
        ];
        private static readonly DSharpLiteralType[][] _typesSortedBySize =
        [
            [
                DSharpLiteralType.Long, DSharpLiteralType.ULong,
                DSharpLiteralType.NInt, DSharpLiteralType.NUInt,
                DSharpLiteralType.Decimal, DSharpLiteralType.Double
            ],
            [DSharpLiteralType.Int, DSharpLiteralType.UInt, DSharpLiteralType.Float],
            [DSharpLiteralType.Short, DSharpLiteralType.UShort, DSharpLiteralType.Char],
            [DSharpLiteralType.Byte, DSharpLiteralType.SByte],
        ];

        /// <summary>
        /// Get larger type between two number types
        /// </summary>
        /// <param name="t1">First number type</param>
        /// <param name="t2">Second number type</param>
        /// <returns>Larger type between two number types</returns>
        public static DSharpLiteralType GetLargerType(DSharpLiteralType t1, DSharpLiteralType t2)
        {
            foreach (var types in _typesSortedBySize)
            {
                if (types.Contains(t1))
                {
                    return t1;
                }
                if (types.Contains(t2))
                {
                    return t2;
                }
            }

            return t1;
        }
        /// <summary>
        /// Perform unary operation with literal value
        /// </summary>
        /// <param name="op">Unary operator</param>
        /// <param name="value">Value to perform operation</param>
        /// <returns>Result of binary operation</returns>
        /// <exception cref="ArgumentException">Not operator (!) available only to boolean value</exception>
        /// <exception cref="ArgumentException">Operator available only to number value</exception>
        /// <exception cref="InvalidOperationException">Unable to get information about type</exception>
        public static DSharpLiteralValue MathOperation(DSharpUnaryOperator op, DSharpLiteralValue value)
        {
            if (op == DSharpUnaryOperator.Not)
            {
                if (!value.IsBool)
                {
                    throw new ArgumentException($"Not operator (!) available only to boolean value, got: {value.Type}", nameof(value));
                }

                return !value.AsBool();
            }
            if (!value.IsNumber)
            {
                throw new ArgumentException($"{(DSharpTokenType)op} operator available only to number value, got: {value.Type}", nameof(value));
            }
            if (!DSharpBuildInTypes.TryGetTypeInfo(value.Type, out var info))
            {
                throw new InvalidOperationException($"Unable to get information about type: {value.Type}");
            }

            var numberType = _standardAssembly.GetType(info.FullName, true);
            var decimalValue = value.AsNumber<decimal>();

            if (op == DSharpUnaryOperator.Minus)
            {
                decimalValue = -decimalValue;
            }
            else if (op == DSharpUnaryOperator.Increment)
            {
                decimalValue++;
            }
            else if (op == DSharpUnaryOperator.Decrement)
            {
                decimalValue--;
            }

            return new()
            {
                Type = value.Type,
                _numberValue = Convert.ChangeType(decimalValue, numberType)
            };
        }
        /// <summary>
        /// Perform binary operation with literal values
        /// </summary>
        /// <param name="op">Binary operator</param>
        /// <param name="n1">First value</param>
        /// <param name="n2">Second value</param>
        /// <returns>Result of binary operation</returns>
        /// <exception cref="ArgumentException">Values must be numbers</exception>
        /// <exception cref="InvalidOperationException">Unable to get information about type</exception>
        public static DSharpLiteralValue MathOperation(DSharpBinaryOperator op, DSharpLiteralValue n1, DSharpLiteralValue n2)
        {
            if (op == DSharpBinaryOperator.LogicalEquals)
            {
                return n1.Equals(n2);
            }
            else if (op == DSharpBinaryOperator.LogicalNotEquals)
            {
                return !n1.Equals(n2);
            }
            else if (op == DSharpBinaryOperator.LogicalXor)
            {
                return n1.AsBool() ^ n2.AsBool();
            }
            else if (op == DSharpBinaryOperator.LogicalOr)
            {
                return n1.AsBool() || n2.AsBool();
            }
            else if (op == DSharpBinaryOperator.LogicalAnd)
            {
                return n1.AsBool() && n2.AsBool();
            }
            if ((!n1.IsNumber || !n2.IsNumber) && op != DSharpBinaryOperator.Plus)
            {
                throw new ArgumentException($"Values must be numbers, got: {n1}, {n2}");
            }
            if (op == DSharpBinaryOperator.LogicalLess)
            {
                return n1.AsNumber<decimal>() < n2.AsNumber<decimal>();
            }
            else if (op == DSharpBinaryOperator.LogicalLessOrEquals)
            {
                return n1.AsNumber<decimal>() <= n2.AsNumber<decimal>();
            }
            else if (op == DSharpBinaryOperator.LogicalGreater)
            {
                return n1.AsNumber<decimal>() > n2.AsNumber<decimal>();
            }
            else if (op == DSharpBinaryOperator.LogicalGreaterOrEquals)
            {
                return n1.AsNumber<decimal>() >= n2.AsNumber<decimal>();
            }

            var largerType = GetLargerType(n1.Type, n2.Type);

            if (_floatingPointTypes.Contains(n1.Type) && !_floatingPointTypes.Contains(n2.Type))
            {
                largerType = n1.Type;
            }
            else if (!_floatingPointTypes.Contains(n1.Type) && _floatingPointTypes.Contains(n2.Type))
            {
                largerType = n2.Type;
            }
            if (!DSharpBuildInTypes.TryGetTypeInfo(largerType, out var info))
            {
                throw new InvalidOperationException($"Unable to get information about type: {largerType}");
            }

            var numberType = _standardAssembly.GetType(info.FullName, true);
            object value = 0;

            if (op == DSharpBinaryOperator.Plus)
            {
                if (n1.IsString && !n2.IsString ||
                    !n1.IsString && n2.IsString ||
                    n1.IsString && n2.IsString)
                {
                    return n1.ToString() + n2.ToString();
                }
                if (!n1.IsNumber || !n2.IsNumber)
                {
                    throw new ArgumentException($"Values must be numbers, got: {n1}, {n2}");
                }

                value = n1.AsNumber<decimal>() + n2.AsNumber<decimal>();
            }
            else if (op == DSharpBinaryOperator.Minus)
            {
                value = n1.AsNumber<decimal>() - n2.AsNumber<decimal>();
            }
            else if (op == DSharpBinaryOperator.Multiply)
            {
                value = n1.AsNumber<decimal>() * n2.AsNumber<decimal>();
            }
            else if (op == DSharpBinaryOperator.Divide)
            {
                value = n1.AsNumber<decimal>() * n2.AsNumber<decimal>();
            }
            else if (op == DSharpBinaryOperator.Mod)
            {
                value = n1.AsNumber<decimal>() % n2.AsNumber<decimal>();
            }
            else if (op == DSharpBinaryOperator.ShiftLeft)
            {
                value = n1.AsNumber<long>() << n2.AsNumber<int>();
            }
            else if (op == DSharpBinaryOperator.ShiftRight)
            {
                value = n1.AsNumber<long>() >> n2.AsNumber<int>();
            }

            return new()
            {
                Type = largerType,
                _numberValue = Convert.ChangeType(value, numberType)
            };
        }
        /// <summary>
        /// Parse number literal
        /// </summary>
        /// <param name="numberLiteral">Number in text format</param>
        public static DSharpLiteralValue Parse(string numberLiteral)
        {
            numberLiteral = numberLiteral.Replace("_", string.Empty).Trim();

            if (numberLiteral.EndsWith("UL", StringComparison.OrdinalIgnoreCase))
            {
                return ParseUnsignedLong(numberLiteral[..^2]);
            }
            if (numberLiteral.EndsWith("LU", StringComparison.OrdinalIgnoreCase))
            {
                return ParseUnsignedLong(numberLiteral[..^2]);
            }
            if (numberLiteral.EndsWith("U", StringComparison.OrdinalIgnoreCase))
            {
                return ParseUnsignedInteger(numberLiteral[..^1]);
            }
            if (numberLiteral.EndsWith("L", StringComparison.OrdinalIgnoreCase))
            {
                return ParseLong(numberLiteral[..^1]);
            }
            if (numberLiteral.EndsWith("F", StringComparison.OrdinalIgnoreCase))
            {
                return ParseFloat(numberLiteral[..^1]);
            }
            if (numberLiteral.EndsWith("D", StringComparison.OrdinalIgnoreCase))
            {
                return ParseDouble(numberLiteral[..^1]);
            }
            if (numberLiteral.EndsWith("M", StringComparison.OrdinalIgnoreCase))
            {
                return ParseDecimal(numberLiteral[..^1]);
            }

            if (numberLiteral.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return ParseHex(numberLiteral[2..]);
            }
            if (numberLiteral.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
            {
                return ParseBinary(numberLiteral[2..]);
            }
            if (numberLiteral.StartsWith("0o", StringComparison.OrdinalIgnoreCase))
            {
                return ParseOctal(numberLiteral[2..]);
            }

            return ParseDecimalAuto(numberLiteral);
        }

        private static DSharpLiteralValue ParseUnsignedLong(string value)
        {
            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong result))
            {
                return result;
            }

            throw new FormatException($"Invalid number format: {value}");
        }
        private static DSharpLiteralValue ParseUnsignedInteger(string value)
        {
            if (uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint result))
            {
                return result;
            }
            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong longResult))
            {
                return longResult;
            }

            throw new FormatException($"Invalid number format: {value}");
        }
        private static DSharpLiteralValue ParseLong(string value)
        {
            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result))
            {
                return result;
            }

            throw new FormatException($"Invalid number format: {value}");
        }
        private static DSharpLiteralValue ParseFloat(string value)
        {
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                return result;
            }

            throw new FormatException($"Invalid number format: {value}");
        }
        private static DSharpLiteralValue ParseDouble(string value)
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }

            throw new FormatException($"Invalid number format: {value}");
        }
        private static DSharpLiteralValue ParseDecimal(string value)
        {
            if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            throw new FormatException($"Invalid number format: {value}");
        }
        private static DSharpLiteralValue ParseHex(string value)
        {
            if (value.Length <= 8)
            {
                if (int.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int intResult))
                {
                    return intResult;
                }
            }
            if (value.Length <= 16)
            {
                if (long.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long longResult))
                {
                    return longResult;
                }
            }

            throw new FormatException($"Invalid hex number format: {value}");
        }
        private static DSharpLiteralValue ParseBinary(string value)
        {
            try
            {
                BigInteger result = 0;

                foreach (char c in value)
                {
                    if (c != '0' && c != '1')
                    {
                        throw new FormatException($"Invalid character in binary number: {c}");
                    }

                    result = (result << 1) | (c - '0');
                }

                if (result <= int.MaxValue && result >= int.MinValue)
                {
                    return (int)result;
                }
                if (result <= long.MaxValue && result >= long.MinValue)
                {
                    return (long)result;
                }
                if (result <= ulong.MaxValue && result >= 0)
                {
                    return (ulong)result;
                }

                return (int)result;
            }
            catch (Exception ex)
            {
                throw new FormatException($"Unable to parse binary number: {ex.Message}");
            }
        }
        private static DSharpLiteralValue ParseOctal(string value)
        {
            try
            {
                BigInteger result = 0;

                foreach (char c in value)
                {
                    if (c < '0' || c > '7')
                    {
                        throw new FormatException($"Invalid character in hex number: {c}");
                    }

                    result = (result << 3) + (c - '0');
                }

                if (result <= int.MaxValue && result >= int.MinValue)
                {
                    return (int)result;
                }
                if (result <= long.MaxValue && result >= long.MinValue)
                {
                    return (long)result;
                }
                if (result <= ulong.MaxValue && result >= 0)
                {
                    return (ulong)result;
                }

                return (int)result;
            }
            catch (Exception ex)
            {
                throw new FormatException($"Unable to parse hex number: {ex.Message}");
            }
        }
        private static DSharpLiteralValue ParseDecimalAuto(string value)
        {
            value = value.Replace(',', '.');

            if (value.Contains('.') || value.Contains('e') || value.Contains('E'))
            {
                bool skipFloat = false;

                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleResult))
                {
                    var textValue = doubleResult.ToString().Replace(',', '.');

                    if (textValue == value)
                    {
                        return doubleResult;
                    }

                    skipFloat = true;
                }
                if (!skipFloat && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatResult))
                {
                    return floatResult;
                }
                if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var decimalResult))
                {
                    return decimalResult;
                }
            }
            else
            {
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intResult))
                {
                    return intResult;
                }
                if (uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uintResult))
                {
                    return uintResult;
                }
                if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longResult))
                {
                    return longResult;
                }
                if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ulongResult))
                {
                    return ulongResult;
                }
            }

            throw new FormatException($"Unable to parse number: {value}");
        }

        #endregion
    }
}

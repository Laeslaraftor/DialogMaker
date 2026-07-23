using static System.Net.Mime.MediaTypeNames;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# external method result
    /// </summary>
    public unsafe readonly struct DSharpExternalMethodResult : IEquatable<DSharpExternalMethodResult>
    {
        /// <summary>
        /// Create D# external method result with literal value
        /// </summary>
        /// <param name="literalValue">Literal value as result</param>
        public DSharpExternalMethodResult(DSharpLiteralValue literalValue)
        {
            _literalValue = literalValue;
        }
        /// <summary>
        /// Create D# external method result with object
        /// </summary>
        /// <param name="pointer">D# object as result</param>
        public DSharpExternalMethodResult(Pointer<DSharpObject> pointer)
        {
            _object = pointer;
        }

        /// <summary>
        /// Is result null
        /// </summary>
        public bool IsNull => _object == null && _literalValue == null;
        /// <summary>
        /// Is result contains literal value
        /// </summary>
        public bool IsLiteralValue => _literalValue != null;
        /// <summary>
        /// Is result contains pointer to D# object
        /// </summary>
        public bool IsObject => _object != null;

        private readonly DSharpLiteralValue? _literalValue;
        private readonly DSharpObject* _object;

        /// <summary>
        /// Get literal value
        /// </summary>
        /// <returns>Literal value</returns>
        /// <exception cref="InvalidOperationException">External method result is not literal value</exception>
        public DSharpLiteralValue AsLiteralValue() => _literalValue ?? throw new InvalidOperationException("External method result is not literal value");
        /// <summary>
        /// Get D# object
        /// </summary>
        /// <returns>D# object</returns>
        /// <exception cref="InvalidOperationException">External method result is not object</exception>
        public Pointer<DSharpObject> AsObject() => _object != null ? _object : throw new InvalidOperationException("External method result is not object");

        public readonly bool Equals(DSharpExternalMethodResult other)
        {
            return _literalValue == other._literalValue &&
                   _object == other._object;
        }
        public readonly override bool Equals(object? obj)
        {
            return obj is DSharpExternalMethodResult other && Equals(other);
        }
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(_literalValue, new Pointer<DSharpObject>(_object));
        }

        public static implicit operator DSharpExternalMethodResult(DSharpLiteralValue literalValue) => new(literalValue);
        public static implicit operator DSharpExternalMethodResult(string? text)
        {
            if (text == null)
            {
                return Null;
            }

            return new(new DSharpLiteralValue(text));
        }
        public static implicit operator DSharpExternalMethodResult(double number) => new(new DSharpLiteralValue(number));
        public static implicit operator DSharpExternalMethodResult(int number) => new(new DSharpLiteralValue(number));
        public static implicit operator DSharpExternalMethodResult(uint number) => new(new DSharpLiteralValue(number));
        public static implicit operator DSharpExternalMethodResult(long number) => new(new DSharpLiteralValue(number));
        public static implicit operator DSharpExternalMethodResult(ulong number) => new(new DSharpLiteralValue(number));
        public static implicit operator DSharpExternalMethodResult(short number) => new(new DSharpLiteralValue(number));
        public static implicit operator DSharpExternalMethodResult(ushort number) => new(new DSharpLiteralValue(number));
        public static implicit operator DSharpExternalMethodResult(byte number) => new(new DSharpLiteralValue(number));
        public static implicit operator DSharpExternalMethodResult(sbyte number) => new(new DSharpLiteralValue(number));
        public static implicit operator DSharpExternalMethodResult(nint number) => new(new DSharpLiteralValue(number));
        public static implicit operator DSharpExternalMethodResult(nuint number) => new(new DSharpLiteralValue(number));
        public static implicit operator DSharpExternalMethodResult(float number) => new(new DSharpLiteralValue(number));
        public static implicit operator DSharpExternalMethodResult(decimal number) => new(new DSharpLiteralValue(number));
        public static implicit operator DSharpExternalMethodResult(bool boolean) => new(new DSharpLiteralValue(boolean));
        public static implicit operator DSharpExternalMethodResult(char character) => new(new DSharpLiteralValue(character));
        public static implicit operator DSharpExternalMethodResult(Pointer<DSharpObject> obj) => new(obj);
        public static implicit operator DSharpExternalMethodResult(DSharpObject* obj) => new(new Pointer<DSharpObject>(obj));

        public static bool operator ==(DSharpExternalMethodResult l, DSharpExternalMethodResult r) => l.Equals(r);
        public static bool operator !=(DSharpExternalMethodResult l, DSharpExternalMethodResult r) => !l.Equals(r);

        #region Static

        /// <summary>
        /// Null result
        /// </summary>
        public static readonly DSharpExternalMethodResult Null = new();

        #endregion
    }
}

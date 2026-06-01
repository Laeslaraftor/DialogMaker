namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Literal value of D#
    /// </summary>
    public readonly struct DSharpLiteralValue
    {
        /// <summary>
        /// Create literal value based on string
        /// </summary>
        /// <param name="text">String as value literal value</param>
        public DSharpLiteralValue(string? text)
        {
            _stringValue = text;
        }
        /// <summary>
        /// Create literal value based on number
        /// </summary>
        /// <param name="text">Number as value literal value</param>
        public DSharpLiteralValue(int number)
        {
            _numberValue = number;
        }
        /// <summary>
        /// Create literal value based on number
        /// </summary>
        /// <param name="text">Number as value literal value</param>
        public DSharpLiteralValue(float number)
        {
            _numberValue = number;
        }
        /// <summary>
        /// Create literal value based on number
        /// </summary>
        /// <param name="text">Number as value literal value</param>
        public DSharpLiteralValue(double number)
        {
            _numberValue = number;
        }
        /// <summary>
        /// Create literal value based on boolean
        /// </summary>
        /// <param name="text">Boolean as value literal value</param>
        public DSharpLiteralValue(bool boolean)
        {
            _boolValue = boolean;
        }

        /// <summary>
        /// Is literal value empty
        /// </summary>
        public bool IsNull => _stringValue == null && _numberValue == null && _boolValue == null;
        /// <summary>
        /// Is literal value string
        /// </summary>
        public bool IsString => _stringValue != null;
        /// <summary>
        /// Is literal value number
        /// </summary>
        public bool IsNumber => _numberValue != null;
        /// <summary>
        /// Is literal value boolean
        /// </summary>
        public bool IsBool => _boolValue != null;

        private readonly string? _stringValue;
        private readonly double? _numberValue;
        private readonly bool? _boolValue;

        #region Управление

        /// <summary>
        /// Get string value of this literal value
        /// </summary>
        /// <returns>String value</returns>
        /// <exception cref="InvalidCastException">Literal value is not a string</exception>
        public string AsString() => _stringValue ?? throw new InvalidCastException("Literal value is not a string");
        /// <summary>
        /// Get number value of this literal value
        /// </summary>
        /// <returns>Number value</returns>
        /// <exception cref="InvalidCastException">Literal value is not a number</exception>
        public double AsNumber() => _numberValue ?? throw new InvalidCastException("Literal value is not a number");
        /// <summary>
        /// Get boolean value of this literal value
        /// </summary>
        /// <returns>Boolean value</returns>
        /// <exception cref="InvalidCastException">Literal value is not a boolean</exception>
        public bool AsBool() => _boolValue ?? throw new InvalidCastException("Literal value is not a boolean");

        #endregion

        #region Операторы

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
        public static implicit operator DSharpLiteralValue(float number) => new(number);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="boolean"><inheritdoc/></param>
        public static implicit operator DSharpLiteralValue(bool boolean) => new(boolean);

        #endregion

        #region Статика

        /// <summary>
        /// Null literal value
        /// </summary>
        public static readonly DSharpLiteralValue Null = new();

        #endregion
    }
}

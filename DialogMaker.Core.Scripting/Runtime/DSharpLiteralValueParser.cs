namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// D# literal value parser
    /// </summary>
    /// <param name="reader">Value reader</param>
    /// <param name="writer">Value writer</param>
    public readonly struct DSharpLiteralValueParser(Func<DSharpStream, DSharpLiteralValue> reader, Action<DSharpStream, DSharpLiteralValue> writer)
    {
        private readonly Func<DSharpStream, DSharpLiteralValue>? _reader = reader;
        private readonly Action<DSharpStream, DSharpLiteralValue>? _writer = writer;

        /// <summary>
        /// Read literal value from stream
        /// </summary>
        /// <param name="stream">Stream that contains literal value</param>
        /// <returns>Literal value that was read</returns>
        public DSharpLiteralValue Read(DSharpStream stream)
        {
            if (_reader != null)
            {
                return _reader(stream);
            }

            return DSharpLiteralValue.Null;
        }
        /// <summary>
        /// Write literal value to stream
        /// </summary>
        /// <param name="stream">Stream for writing literal value</param>
        /// <param name="value">Literal value for writing</param>
        public void Write(DSharpStream stream, DSharpLiteralValue value)
        {
            _writer?.Invoke(stream, value);
        }
    }
}

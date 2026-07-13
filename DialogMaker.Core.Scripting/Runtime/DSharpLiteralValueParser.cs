namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// D# literal value parser
    /// </summary>
    /// <param name="reader">Value reader</param>
    /// <param name="writer">Value writer</param>
    public readonly struct DSharpLiteralValueParser(Func<Stream, DSharpLiteralValue> reader, Action<Stream, DSharpLiteralValue> writer)
    {
        private readonly Func<Stream, DSharpLiteralValue>? _reader = reader;
        private readonly Action<Stream, DSharpLiteralValue>? _writer = writer;

        /// <summary>
        /// Read literal value from stream
        /// </summary>
        /// <param name="stream">Stream that contains literal value</param>
        /// <returns>Literal value that was read</returns>
        public DSharpLiteralValue Read(Stream stream)
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
        public void Write(Stream stream, DSharpLiteralValue value)
        {
            _writer?.Invoke(stream, value);
        }
    }
}

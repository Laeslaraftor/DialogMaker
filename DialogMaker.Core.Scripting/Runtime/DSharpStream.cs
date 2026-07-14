using static DialogMaker.Core.Scripting.Runtime.DSharpStream;
using DialogMaker.Core.Scripting.Runtime.Executor;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Structure of stream that provides remove controls to any stream.
    /// Used for <see cref="Stream"/> and <see cref="UnmanagedStream"/>
    /// </summary>
    /// <param name="byteReader">Byte reader</param>
    /// <param name="reader">Buffer reader</param>
    /// <param name="byteWriter">Byte writer</param>
    /// <param name="writer">Buffer writer</param>
    /// <param name="positionGetter">Position getter</param>
    /// <param name="positionSetter">Position setter</param>
    /// <param name="lengthGetter">Length getter</param>
    public readonly struct DSharpStream(ByteReader byteReader, Reader reader, ByteWriter byteWriter, Writer writer, Func<long> positionGetter, Action<long> positionSetter, Func<long> lengthGetter)
    {
        /// <summary>
        /// Current stream position
        /// </summary>
        public long Position
        {
            get => _positionGetter();
            set => _positionSetter(value);
        }
        /// <summary>
        /// Stream total length
        /// </summary>
        public long Length => _lengthGetter();

        private readonly ByteReader _byteReader = byteReader;
        private readonly Reader _reader = reader;
        private readonly ByteWriter _byteWriter = byteWriter;
        private readonly Writer _writer = writer;
        private readonly Func<long> _positionGetter = positionGetter;
        private readonly Action<long> _positionSetter = positionSetter;
        private readonly Func<long> _lengthGetter = lengthGetter;

        #region Controls

        /// <summary>
        /// <inheritdoc cref="Stream.ReadByte"/>
        /// </summary>
        /// <returns><inheritdoc cref="Stream.ReadByte"/></returns>
        public int ReadByte() => _byteReader();
        /// <summary>
        /// <inheritdoc cref="Stream.Read(Span{byte})"/>
        /// </summary>
        /// <param name="buffer"><inheritdoc cref="Stream.Read(Span{byte})"/></param>
        /// <returns><inheritdoc cref="Stream.Read(Span{byte})"/></returns>
        public int Read(Span<byte> buffer) => _reader(buffer);
        /// <summary>
        /// <inheritdoc cref="Stream.WriteByte(byte)"/>
        /// </summary>
        /// <param name="value"><inheritdoc cref="Stream.WriteByte(byte)"/></param>
        public void WriteByte(byte value) => _byteWriter(value);
        /// <summary>
        /// <inheritdoc cref="Stream.Write(ReadOnlySpan{byte})"/>
        /// </summary>
        /// <param name="buffer"><inheritdoc cref="Stream.Write(ReadOnlySpan{byte})"/></param>
        public void Write(ReadOnlySpan<byte> buffer) => _writer(buffer);

        #endregion

        #region Delegates

        /// <summary>
        /// Byte reader delegate
        /// </summary>
        /// <returns></returns>
        public delegate int ByteReader();
        /// <summary>
        /// Buffer reader delegate
        /// </summary>
        /// <param name="buffer">Buffer for writing values from stream</param>
        /// <returns>Amount of bytes that was read</returns>
        public delegate int Reader(Span<byte> buffer);
        /// <summary>
        /// Byte writer delegate
        /// </summary>
        /// <param name="value">Value for writing to stream</param>
        public delegate void ByteWriter(byte value);
        /// <summary>
        /// Buffer writer delegate
        /// </summary>
        /// <param name="value">Buffer for writing to stream</param>
        public delegate void Writer(ReadOnlySpan<byte> value);

        #endregion

        #region Operators

        public static implicit operator DSharpStream(Stream stream)
        {
            return new(stream.ReadByte, stream.Read, stream.WriteByte, stream.Write, () => stream.Position, p => stream.Position = p, () => stream.Length);
        }
        public static implicit operator DSharpStream(UnmanagedStream stream)
        {
            return new(() =>
            {
                if (stream.Position < stream.Length)
                {
                    return stream.Read<byte>();
                }

                return -1;
            }, stream.Read, v => stream.Write(v), b => stream.WriteBuffer(b), () => stream.Position, p => stream.Position = (int)p, () => stream.Length);
        }

        #endregion
    }
}

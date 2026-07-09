using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Metadata token of D# type
    /// </summary>
    /// <param name="type">Metadata token type</param>
    /// <param name="index">Metadata token index</param>
    /// <param name="assemblyIndex">Assembly index in references table. 0 means that type stores in current assembly</param>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct DSharpMetadataToken(DSharpMetadataTokenType type, int index, int assemblyIndex) : IEquatable<DSharpMetadataToken>
    {
        /// <summary>
        /// Create new metadata token based on other
        /// </summary>
        /// <param name="token">Original metadata token</param>
        /// <param name="assemblyIndex">Assembly index in references table. 0 means that type stores in current assembly</param>
        public DSharpMetadataToken(DSharpMetadataToken token, int assemblyIndex)
            : this(token.Type, token.Index, assemblyIndex)
        {
        }

        /// <summary>
        /// Value of this token
        /// </summary>
        public int Value => _value;
        /// <summary>
        /// Assembly index in references table. 0 means that type stores in current assembly
        /// </summary>
        public int AssemblyIndex => _assemblyIndex;
        /// <summary>
        /// Index of this token
        /// </summary>
        public int Index => _value & 0x00FFFFFF;
        /// <summary>
        /// Type of this token
        /// </summary>
        public DSharpMetadataTokenType Type => (DSharpMetadataTokenType)(_value & 0xFF000000);

        private readonly int _value = (int)type | index;
        private readonly int _assemblyIndex = assemblyIndex;

        #region Управление

        /// <summary>
        /// Write metadata token to stream
        /// </summary>
        /// <param name="stream">Stream for writing current token</param>
        public void Write(Stream stream)
        {
            var valueBytes = BitConverter.GetBytes(_value);
            var assemblyIndexBytes = BitConverter.GetBytes(_assemblyIndex);

            stream.Write(valueBytes);
            stream.Write(assemblyIndexBytes);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="other"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public bool Equals(DSharpMetadataToken other)
        {
            return Value == other.Value;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public override bool Equals(object obj)
        {
            return obj is DSharpMetadataToken other &&
                   Equals(other);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString() => $"{Type}:0x{Value:X8}";

        #endregion

        #region Операторы

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="l"><inheritdoc/></param>
        /// <param name="r"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator ==(DSharpMetadataToken l, DSharpMetadataToken r) => l.Equals(r);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="l"><inheritdoc/></param>
        /// <param name="r"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator !=(DSharpMetadataToken l, DSharpMetadataToken r) => !l.Equals(r);

        #endregion
    }
}

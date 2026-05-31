namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Metadata token of D# type
    /// </summary>
    /// <param name="type">Metadata token type</param>
    /// <param name="index">Metadata token index</param>
    /// <param name="assemblyIndex">Assembly index in references table. 0 means that type stores in current assembly</param>
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
        public int Value { get; } = (int)type | index;
        /// <summary>
        /// Assembly index in references table. 0 means that type stores in current assembly
        /// </summary>
        public int AssemblyIndex { get; } = assemblyIndex;
        /// <summary>
        /// Index of this token
        /// </summary>
        public int Index => Value & 0x00FFFFFF;
        /// <summary>
        /// Type of this token
        /// </summary>
        public DSharpMetadataTokenType Type => (DSharpMetadataTokenType)(Value & 0xFF000000);

        #region Управление

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

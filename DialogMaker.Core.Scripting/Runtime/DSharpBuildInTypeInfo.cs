using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Information about build-in type
    /// </summary>
    /// <param name="fullName">Full name of type</param>
    /// <param name="size">Size of type in bytes</param>
    /// <param name="token">Token that represents type</param>
    /// <param name="shortName">Short name of type</param>
    public readonly struct DSharpBuildInTypeInfo(string fullName, int size, DSharpTokenType? token, string? shortName)
        : IEquatable<DSharpBuildInTypeInfo>
    {
        /// <summary>
        /// Create information about build-in type
        /// </summary>
        /// <param name="fullName">Full name of type</param>
        /// <param name="size">Size of type in bytes</param>
        /// <param name="shortName">Short name of type</param>
        public DSharpBuildInTypeInfo(string fullName, int size, string shortName)
            : this(fullName, size, null, shortName)
        {
        }
        /// <summary>
        /// Create information about build-in type
        /// </summary>
        /// <param name="fullName">Full name of type</param>
        /// <param name="size">Size of type in bytes</param>
        /// <param name="token">Token that represents type</param>
        public DSharpBuildInTypeInfo(string fullName, int size, DSharpTokenType token)
            : this(fullName, size, token, token.ToString().ToLower())
        {
        }
        /// <summary>
        /// Create information about build-in type
        /// </summary>
        /// <param name="fullName">Full name of type</param>
        /// <param name="size">Size of type in bytes</param>
        public DSharpBuildInTypeInfo(string fullName, int size)
            : this(fullName, size, null, null)
        {
        }
        /// <summary>
        /// Create information about build-in type
        /// </summary>
        /// <param name="fullName">Full name of type</param>
        public DSharpBuildInTypeInfo(string fullName)
            : this(fullName, -1, null, null)
        {
        }

        /// <summary>
        /// Full name of type
        /// </summary>
        public string FullName { get; } = fullName;
        /// <summary>
        /// Short name of type
        /// </summary>
        public string? ShortName { get; } = shortName;
        /// <summary>
        /// Size of type in bytes
        /// </summary>
        public int Size { get; } = size;
        /// <summary>
        /// Token that represents type
        /// </summary>
        public DSharpTokenType? Token { get; } = token;

        #region Операторы

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="l"><inheritdoc/></param>
        /// <param name="r"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator ==(DSharpBuildInTypeInfo l, DSharpBuildInTypeInfo r) => l.Equals(r);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="l"><inheritdoc/></param>
        /// <param name="r"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator !=(DSharpBuildInTypeInfo l, DSharpBuildInTypeInfo r) => !l.Equals(r);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="info"><inheritdoc/></param>
        public static implicit operator string(DSharpBuildInTypeInfo info) => info.FullName;

        #endregion

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="other"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public bool Equals(DSharpBuildInTypeInfo other)
        {
            return FullName == other.FullName &&
                   ShortName == other.ShortName &&
                   Size == other.Size &&
                   Token == other.Token;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public override bool Equals(object obj)
        {
            return obj is DSharpBuildInTypeInfo other && Equals(other);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(FullName, Size);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return FullName;
        }

        #endregion
    }
}

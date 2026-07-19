using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime.Executor;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Information about build-in type
    /// </summary>
    public unsafe readonly struct DSharpBuildInTypeInfo : IEquatable<DSharpBuildInTypeInfo>
    {
        /// <param name="fullName">Full name of type</param>
        /// <param name="size">Size of type in bytes</param>
        /// <param name="token">Token that represents type</param>
        /// <param name="shortName">Short name of type</param>
        public DSharpBuildInTypeInfo(string fullName, int size, DSharpTokenType? token, string? shortName, delegate*<DSharpObject*, object> converter)
        {
            FullName = fullName;
            ShortName = shortName;
            Size = size;
            Token = token;
            Converter = converter;

            var parts = fullName.Split('.');
            Name = parts[^1];
            Namespace = fullName[..(fullName.Length - Name.Length - 1)];
        }

        /// <summary>
        /// Create information about build-in type
        /// </summary>
        /// <param name="fullName">Full name of type</param>
        /// <param name="size">Size of type in bytes</param>
        /// <param name="shortName">Short name of type</param>
        public DSharpBuildInTypeInfo(string fullName, int size, string shortName)
            : this(fullName, size, null, shortName, null)
        {
        }
        /// <summary>
        /// Create information about build-in type
        /// </summary>
        /// <param name="fullName">Full name of type</param>
        /// <param name="size">Size of type in bytes</param>
        /// <param name="token">Token that represents type</param>
        public DSharpBuildInTypeInfo(string fullName, int size, DSharpTokenType token, delegate*<DSharpObject*, object> converter)
            : this(fullName, size, token, token.ToString().ToLower(), converter)
        {
        }
        /// <summary>
        /// Create information about build-in type
        /// </summary>
        /// <param name="fullName">Full name of type</param>
        /// <param name="size">Size of type in bytes</param>
        public DSharpBuildInTypeInfo(string fullName, int size)
            : this(fullName, size, null, null, null)
        {
        }
        /// <summary>
        /// Create information about build-in type
        /// </summary>
        /// <param name="fullName">Full name of type</param>
        public DSharpBuildInTypeInfo(string fullName)
            : this(fullName, -1, null, null, null)
        {
        }

        /// <summary>
        /// Full name of type (namespace + type name)
        /// </summary>
        public string FullName { get; }
        /// <summary>
        /// Type namespace
        /// </summary>
        public string Namespace { get; }
        /// <summary>
        /// Type name
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Short name of type (keyword)
        /// </summary>
        public string? ShortName { get; }
        /// <summary>
        /// Size of type in bytes
        /// </summary>
        public int Size { get; }
        /// <summary>
        /// Token that represents type
        /// </summary>
        public DSharpTokenType? Token { get; }
        /// <summary>
        /// D# object converter to C# object
        /// </summary>
        public delegate*<DSharpObject*, object> Converter { get; }

        #region Operators

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

        #region Controls

        /// <summary>
        /// Convert D# object to C# object
        /// </summary>
        /// <param name="obj">D# object instance</param>
        /// <returns>C# object</returns>
        /// <exception cref="InvalidOperationException">Type has not converter</exception>
        public T Convert<T>(DSharpObject* obj)
        {
            var convertedValue = Convert(obj);
            return (T)System.Convert.ChangeType(convertedValue, typeof(T));
        }
        /// <summary>
        /// Convert D# object to C# object
        /// </summary>
        /// <param name="obj">D# object instance</param>
        /// <returns>C# object</returns>
        /// <exception cref="InvalidOperationException">Type has not converter</exception>
        public object Convert(DSharpObject* obj)
        {
            if (Converter == null)
            {
                throw new InvalidOperationException($"Type \"{this}\" has not converter");
            }

            return Converter(obj);
        }

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
        public override bool Equals(object? obj)
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

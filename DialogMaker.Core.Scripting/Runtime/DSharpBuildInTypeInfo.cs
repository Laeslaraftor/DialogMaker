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
        public DSharpBuildInTypeInfo(string fullName, int size, DSharpTokenType? token, DSharpLiteralType? literalType, string? shortName, delegate*<DSharpObject*, DSharpNumber> converter)
        {
            FullName = fullName;
            ShortName = shortName;
            Size = size;
            Token = token;
            LiteralType = literalType;
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
            : this(fullName, size, null, null, shortName, null)
        {
        }
        /// <summary>
        /// Create information about build-in type
        /// </summary>
        /// <param name="fullName">Full name of type</param>
        /// <param name="size">Size of type in bytes</param>
        /// <param name="token">Token that represents type</param>
        public DSharpBuildInTypeInfo(string fullName, int size, DSharpTokenType token, DSharpLiteralType? literalType, delegate*<DSharpObject*, DSharpNumber> converter)
            : this(fullName, size, token, literalType, token.ToString().ToLower(), converter)
        {
        }
        /// <summary>
        /// Create information about build-in type
        /// </summary>
        /// <param name="fullName">Full name of type</param>
        /// <param name="size">Size of type in bytes</param>
        /// <param name="token">Token that represents type</param>
        /// <param name="minMax">Minimal and maximal value of current type</param>
        public DSharpBuildInTypeInfo(string fullName, int size, DSharpTokenType token, DSharpLiteralType literalType, delegate*<DSharpObject*, DSharpNumber> converter, MinMaxValues minMax)
            : this(fullName, size, token, literalType, token.ToString().ToLower(), converter)
        {
            MinMax = minMax;
        }
        /// <summary>
        /// Create information about build-in type
        /// </summary>
        /// <param name="fullName">Full name of type</param>
        /// <param name="size">Size of type in bytes</param>
        public DSharpBuildInTypeInfo(string fullName, int size)
            : this(fullName, size, null, null, null, null)
        {
        }
        /// <summary>
        /// Create information about build-in type
        /// </summary>
        /// <param name="fullName">Full name of type</param>
        public DSharpBuildInTypeInfo(string fullName)
            : this(fullName, -1, null, null, null, null)
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
        /// Literal type that represents type
        /// </summary>
        public DSharpLiteralType? LiteralType { get; }
        /// <summary>
        /// D# object converter to C# object
        /// </summary>
        public delegate*<DSharpObject*, DSharpNumber> Converter { get; }
        /// <summary>
        /// Minimal and maximal value of current type
        /// </summary>
        public MinMaxValues? MinMax { get; }

        #region Operators

        public static bool operator ==(DSharpBuildInTypeInfo l, DSharpBuildInTypeInfo r) => l.Equals(r);
        public static bool operator !=(DSharpBuildInTypeInfo l, DSharpBuildInTypeInfo r) => !l.Equals(r);
        public static implicit operator string(DSharpBuildInTypeInfo info) => info.FullName;

        #endregion

        #region Controls

        /// <summary>
        /// Convert D# object to C# object
        /// </summary>
        /// <param name="obj">D# object instance</param>
        /// <returns>C# object</returns>
        /// <exception cref="InvalidOperationException">Type is not a number</exception>
        public T Convert<T>(DSharpObject* obj) where T : unmanaged
        {
            try
            {
                return Convert(obj).Convert<T>();
            }
            catch (Exception error)
            {
                throw new InvalidOperationException($"Type \"{this}\" is not a number", error);
            }
        }
        /// <summary>
        /// Convert D# object to C# object
        /// </summary>
        /// <param name="obj">D# object instance</param>
        /// <returns>C# object</returns>
        /// <exception cref="InvalidOperationException">Type has no converter</exception>
        public DSharpNumber Convert(DSharpObject* obj)
        {
            if (Converter == null)
            {
                throw new InvalidOperationException($"Type \"{this}\" has no converter");
            }

            return Converter(obj);
        }

        public bool Equals(DSharpBuildInTypeInfo other)
        {
            return FullName == other.FullName &&
                   ShortName == other.ShortName &&
                   Size == other.Size &&
                   Token == other.Token &&
                   LiteralType == other.LiteralType;
        }
        public override bool Equals(object? obj)
        {
            return obj is DSharpBuildInTypeInfo other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(FullName, Size);
        }
        public override string ToString()
        {
            return FullName;
        }

        #endregion

        #region Structs

        public record struct MinMaxValues(double Min, double Max, Type Type);

        #endregion
    }
}

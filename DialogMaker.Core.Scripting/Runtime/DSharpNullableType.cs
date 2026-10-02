namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Information about D# System.Nullable`1 type
    /// </summary>
    public class DSharpNullableType(IDSharpType type, IDSharpType memberType, IDSharpPropertyInfo hasValueProperty, IDSharpPropertyInfo valueProperty, IDSharpMethodInfo constructor)
    {
        /// <summary>
        /// Nullable type
        /// </summary>
        public IDSharpType Type { get; } = type;
        /// <summary>
        /// Type that stored in nullable
        /// </summary>
        public IDSharpType MemberType { get; } = memberType;
        /// <summary>
        /// Property that indicated value existence
        /// </summary>
        public IDSharpPropertyInfo HasValueProperty { get; } = hasValueProperty;
        /// <summary>
        /// Property that contains value
        /// </summary>
        public IDSharpPropertyInfo ValueProperty { get; } = valueProperty;
        /// <summary>
        /// .ctor(T value, bool hasValue)
        /// </summary>
        public IDSharpMethodInfo Constructor { get; } = constructor;

        #region Static

        /// <summary>
        /// Create information of D# System.Nullable`1 type
        /// </summary>
        /// <param name="assembly">Assembly for searching System.Nullable`1 type</param>
        /// <returns>Information about D# System.Nullable`1 type</returns>
        public static DSharpNullableType Create(IDSharpAssembly assembly)
        {
            var type = assembly.GetType(DSharpBuildInTypes.Nullable);
            return Create(type);
        }
        /// <summary>
        /// Create information of D# System.Nullable`1 type
        /// </summary>
        /// <param name="type">D# System.Nullable`1 type that will be used for creating information</param>
        /// <returns>Information about D# System.Nullable`1 type</returns>
        /// <exception cref="ArgumentException">Type not contains required constructor (T value, bool hasValue)</exception>
        public static DSharpNullableType Create(IDSharpType type)
        {
            var hasValueProperty = type.GetProperty("HasValue");
            var valueProperty = GetValueProperty(type);
            var constructor = type.GetConstructors().FirstOrDefault()
                ?? throw new ArgumentException($"Type \"{type}\" not contains required constructor (T value, bool hasValue)");
            var memberType = (type.GetGenericParameters().FirstOrDefault() ?? type.GetGenericTypes().FirstOrDefault())
                ?? throw new ArgumentException($"Type \"{type}\" not contains generic parameter");

            return new(type, memberType, hasValueProperty, valueProperty, constructor);
        }
        /// <summary>
        /// Get property that contains value
        /// </summary>
        /// <param name="type">Nullable type</param>
        /// <returns>Property that contains value</returns>
        public static IDSharpPropertyInfo GetValueProperty(IDSharpType type) => type.GetProperty("Value");

        #endregion
    }
}

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Attributes of generic type
    /// </summary>
    [Flags]
    public enum DSharpGenericTypeAttributes
    {
        /// <summary>
        /// Empty attribute
        /// </summary>
        None = 1 << 0,
        /// <summary>
        /// Attribute that requires constructor with no parameter
        /// </summary>
        EmptyConstructor = 1 << 1,
        /// <summary>
        /// Attribute that requires value type (structures)
        /// </summary>
        Struct = 1 << 2,
        /// <summary>
        /// Attribute that disallows nullable types (types with ? at end)
        /// </summary>
        NotNull = 1 << 3,
    }
}

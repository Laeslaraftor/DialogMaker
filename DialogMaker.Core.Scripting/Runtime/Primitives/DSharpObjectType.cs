namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Type of object
    /// </summary>
    public enum DSharpObjectType
    {
        /// <summary>
        /// <inheritdoc cref="DSharpTypeAttributes.Class"/>
        /// </summary>
        Class = DSharpTypeAttributes.Class,
        /// <summary>
        /// <inheritdoc cref="DSharpTypeAttributes.Struct"/>
        /// </summary>
        Struct = DSharpTypeAttributes.Struct,
        /// <summary>
        /// <inheritdoc cref="DSharpTypeAttributes.Enum"/>
        /// </summary>
        Enum = DSharpTypeAttributes.Enum,
        /// <summary>
        /// <inheritdoc cref="DSharpTypeAttributes.Interface"/>
        /// </summary>
        Interface = DSharpTypeAttributes.Interface
    }
}

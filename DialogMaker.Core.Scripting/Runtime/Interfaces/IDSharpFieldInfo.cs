namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Interface of D# field
    /// </summary>
    public interface IDSharpFieldInfo : IDSharpMemberInfo
    {
        /// <summary>
        /// Type of value that contains by current field
        /// </summary>
        public IDSharpType FieldType { get; }
        /// <summary>
        /// Is current field available only for reading
        /// </summary>
        public bool IsReadOnly { get; }
        /// <summary>
        /// Precompiled value that contains current field by default
        /// </summary>
        public DSharpLiteralValue? RawValue { get; }
    }
}

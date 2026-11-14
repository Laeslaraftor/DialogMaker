namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// D# type size load mode
    /// </summary>
    public enum DSharpLoadTypeSizeMode : byte
    {
        /// <summary>
        /// Load type total size (sum of field type sizes)
        /// </summary>
        TotalSize,
        /// <summary>
        /// Load reference size. 
        /// For value types returned value will be equals to <see cref="TotalSize"/>,
        /// reference types returns pointer size
        /// </summary>
        ReferenceSize
    }
}

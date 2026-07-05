namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    /// <summary>
    /// Type names check mode
    /// </summary>
    public enum TypeNameEqualityCheckMode
    {
        /// <summary>
        /// Full check: short name, declaring types, namespace
        /// </summary>
        Full,
        /// <summary>
        /// Check only by short name
        /// </summary>
        OnlyShortName,
        /// <summary>
        /// Check all without short name
        /// </summary>
        SkipShortName
    }
}

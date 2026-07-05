namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    /// <summary>
    /// Type of type name equality
    /// </summary>
    public enum TypeNameEqualityType
    {
        /// <summary>
        /// Type name not equals
        /// </summary>
        None,
        /// <summary>
        /// Equals by type name
        /// </summary>
        Name,
        /// <summary>
        /// Equals by type name that contains declaring types
        /// </summary>
        DeclaringTypes,
        /// <summary>
        /// Equals by type ame that contains declaring types and namespace
        /// </summary>
        Namespace
    }
}

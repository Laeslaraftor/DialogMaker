namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Type of value that stores in stack
    /// </summary>
    public enum DSharpStackValueType : byte
    {
        /// <summary>
        /// Null value
        /// </summary>
        Null,
        /// <summary>
        /// Value type
        /// </summary>
        Structure,
        /// <summary>
        /// Reference to object
        /// </summary>
        Reference,
        /// <summary>
        /// Information about method calling
        /// </summary>
        MethodCallingInfo,
        /// <summary>
        /// Buffer for method parameters values
        /// </summary>
        MethodParametersBuffer,
        /// <summary>
        /// Scope
        /// </summary>
        Scope
    }
}

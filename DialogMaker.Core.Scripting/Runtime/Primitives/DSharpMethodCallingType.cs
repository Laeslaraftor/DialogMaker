namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Type of method calling
    /// </summary>
    public enum DSharpMethodCallingType : byte
    {
        /// <summary>
        /// Default calling. This calling don't check method overriding
        /// </summary>
        Default,
        /// <summary>
        /// Virtual calling. This calling trying to find method overriding
        /// </summary>
        Virtual
    }
}

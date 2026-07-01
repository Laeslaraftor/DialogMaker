namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Cast availability status
    /// </summary>
    public enum DSharpCastAvailability
    {
        /// <summary>
        /// Cast not available
        /// </summary>
        No,
        /// <summary>
        /// Cast can be performed silently
        /// </summary>
        Implicit,
        /// <summary>
        /// Cast can be performed and should be explicit
        /// </summary>
        Explicit
    }
}

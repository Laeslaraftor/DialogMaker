namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Interface of external methods provider.
    /// </summary>
    public interface IDSharpExternalMethodsProvider
    {
        /// <summary>
        /// Get delegate for execute external method
        /// </summary>
        /// <param name="methodInfo">External method that requires external implementation</param>
        /// <returns>Delegate for executing external method. If method is unknown it returns null</returns>
        public Delegate? GetMethod(IDSharpMethodInfo methodInfo);
    }
}

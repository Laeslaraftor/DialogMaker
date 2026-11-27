namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Base class of .NET object wrapper for D#
    /// </summary>
    public abstract class DSharpDotnetObject(int id) : DSharpDisposable, IDSharpExternalMethodsProvider
    {
        /// <summary>
        /// Wrapper id
        /// </summary>
        public int Id { get; } = id;

        private readonly Dictionary<IDSharpMethodInfo, DSharpExternalMethod?> _cachedDelegates = [];

        public DSharpExternalMethod? GetMethod(IDSharpMethodInfo methodInfo)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            if (!_cachedDelegates.TryGetValue(methodInfo, out var result))
            {
                result = GetMethodDelegate(methodInfo);
                _cachedDelegates.Add(methodInfo, result);
            }

            return result;
        }

        /// <summary>
        /// Get .NET object method with same signature. 
        /// All returned values will be cached
        /// </summary>
        /// <param name="methodInfo">D# representation of required .NET method</param>
        /// <returns>Delegate of required .NET method or null when method is unknown</returns>
        protected abstract DSharpExternalMethod? GetMethodDelegate(IDSharpMethodInfo methodInfo);
    }
}

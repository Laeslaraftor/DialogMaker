namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Interface of D# indexer
    /// </summary>
    public interface IDSharpIndexerInfo : IDSharpPropertyInfo, IDSharpCallableMember
    {
        /// <summary>
        /// Get array of parameters for invoke current indexer
        /// </summary>
        /// <returns>Array of parameters for invoke current indexer</returns>
        public new IDSharpParameterInfo[] GetParameters();
    }
}

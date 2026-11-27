namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Interface of .NET object wrappers for D#
    /// </summary>
    public interface IDSharpDotnetObjectsFabric
    {
        /// <summary>
        /// Create .NET object wrapper with specified type
        /// </summary>
        /// <param name="id">Creating wrapper id. Returned wrapper should have this id</param>
        /// <param name="typeName">Full type name for create object instance with it</param>
        /// <returns>.NET object wrapper or null when type is unknown</returns>
        public DSharpDotnetObject? Create(int id, string typeName);
    }
}

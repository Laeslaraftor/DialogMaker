namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Interface of D# assembly
    /// </summary>
    public interface IDSharpAssembly
    {
        /// <summary>
        /// Name of assembly
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// All types in current assembly
        /// </summary>
        public IReadOnlyCollection<IDSharpType> Types { get; }
        /// <summary>
        /// Root type of all D# object (<see cref="DSharpBuildInTypes.Object"/>)
        /// </summary>
        public IDSharpType ObjectType { get; }

        /// <summary>
        /// Get assembly global variables
        /// </summary>
        /// <returns>Array of global variables</returns>
        public IDSharpFieldInfo[] GetGlobalVariables();
        /// <summary>
        /// Get assembly global functions
        /// </summary>
        /// <returns>Array of global functions</returns>
        public IDSharpMethodInfo[] GetGlobalFunctions();

        /// <summary>
        /// Get member by it's metadata token
        /// </summary>
        /// <param name="metadataToken">Member metadata token</param>
        /// <returns>Member with specified metadata token</returns>
        public IDSharpMemberInfo GetType(DSharpMetadataToken metadataToken);
        /// <summary>
        /// Get type by it full name
        /// </summary>
        /// <param name="fullName">Full name of type</param>
        /// <returns>Type with same full name</returns>
        public IDSharpType GetType(string fullName);
        public List<IDSharpType> GetTypes(string fullName);
        public List<IDSharpType> GetTypes(string? @namespace, string name);
    }
}

namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpAssembly
    {
        public IReadOnlyCollection<IDSharpType> Types { get; }
        public IDSharpType ObjectType { get; }

        public IDSharpFieldInfo[] GetGlobalVariables();
        public IDSharpMethodInfo[] GetGlobalFunctions();

        public IDSharpMemberInfo GetType(DSharpMetadataToken metadataToken);
        public IDSharpType GetType(string fullName);
        public List<IDSharpType> GetTypes(string fullName);
    }
}

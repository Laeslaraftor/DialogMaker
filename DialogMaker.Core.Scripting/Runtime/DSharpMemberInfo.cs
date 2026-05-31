namespace DialogMaker.Core.Scripting.Runtime
{
    public abstract class DSharpMemberInfo
    {
        public abstract DSharpAssembly Assembly { get; }
        public DSharpMetadataToken MetadataToken { get; }
        public abstract string Name { get; }
        public abstract string FullName { get; }
        public abstract DSharpType? DeclaringType { get; }
    }
}

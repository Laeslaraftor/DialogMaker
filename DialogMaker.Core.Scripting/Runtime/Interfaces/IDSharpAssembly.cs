namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpAssembly
    {
        public IDSharpMemberInfo GetType(DSharpMetadataToken metadataToken);
    }
}

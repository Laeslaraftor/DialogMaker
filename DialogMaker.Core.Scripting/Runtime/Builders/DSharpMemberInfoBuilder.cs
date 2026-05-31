namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public abstract class DSharpMemberInfoBuilder(DSharpAssemblyBuilder assembly, string name, DSharpMetadataToken metadataToken)
    {
        public DSharpAssemblyBuilder Assembly { get; } = assembly;
        public string Name { get; } = name;
        public DSharpMetadataToken MetadataToken { get; internal set; } = metadataToken;
        public DSharpTypeBuilder? DeclaringType { get; set; }
        public List<DSharpAttributeDataBuilder> Attributes { get; } = [];
    }
}

namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public class DSharpTypeBuilder(DSharpAssemblyBuilder assembly, string name, DSharpMetadataToken metadataToken) 
        : DSharpMemberInfoBuilder(assembly, name, metadataToken)
    {
        public string? Namespace { get; set; }
    }
}

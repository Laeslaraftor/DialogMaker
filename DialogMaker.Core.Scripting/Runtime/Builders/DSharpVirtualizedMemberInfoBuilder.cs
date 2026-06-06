namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public abstract class DSharpVirtualizedMemberInfoBuilder(DSharpAssemblyBuilder assembly, string name, DSharpTypeToken metadataToken)
        : DSharpMemberInfoBuilder(assembly, name, metadataToken)
    {
        public virtual bool IsAbstract { get; set; }
        public virtual bool IsSealed { get; set; }
        public virtual bool IsOverride { get; set; }
    }
}

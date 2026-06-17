namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpPropertyInfo : IDSharpMemberInfo
    {
        public IDSharpType PropertyType { get; }
        public IDSharpMethodInfo? Getter { get; }
        public IDSharpMethodInfo? Setter { get; }
        public bool CanRead { get; }
        public bool CanWrite { get; }
        public bool IsVirtual { get; }
        public bool IsOverride { get; }
        public bool IsSealed { get; }
    }
}

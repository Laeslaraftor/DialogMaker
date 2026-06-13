namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpPropertyInfo : IDSharpMemberInfo
    {
        public IDSharpType PropertyType { get; }
        public IDSharpMethodInfo? GetterMethod { get; }
        public IDSharpMethodInfo? SetterMethod { get; }
        public bool CanRead { get; }
        public bool CanWrite { get; }
        public bool IsVirtual { get; }
        public bool IsOverride { get; }
        public bool IsSealed { get; }
    }
}

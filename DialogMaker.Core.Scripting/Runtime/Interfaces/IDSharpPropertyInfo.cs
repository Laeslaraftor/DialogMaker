namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpPropertyInfo : IDSharpMemberInfo
    {
        public IDSharpType PropertyType { get; }
        public bool CanRead { get; }
        public bool CanWrite { get; }
    }
}

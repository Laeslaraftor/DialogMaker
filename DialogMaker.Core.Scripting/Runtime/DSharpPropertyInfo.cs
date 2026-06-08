namespace DialogMaker.Core.Scripting.Runtime
{
    public abstract class DSharpPropertyInfo : DSharpMemberInfo, IDSharpPropertyInfo
    {
        public IDSharpType PropertyType => throw new NotImplementedException();
        public bool CanRead => throw new NotImplementedException();
        public bool CanWrite => throw new NotImplementedException();
    }
}

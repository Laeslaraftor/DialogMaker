namespace DialogMaker.Core.Scripting.Runtime
{
    public abstract class DSharpFieldInfo : DSharpMemberInfo, IDSharpFieldInfo
    {
        public IDSharpType FieldType => throw new NotImplementedException();
        public bool IsReadOnly => throw new NotImplementedException();
        public DSharpLiteralValue? RawValue => throw new NotImplementedException();
    }
}

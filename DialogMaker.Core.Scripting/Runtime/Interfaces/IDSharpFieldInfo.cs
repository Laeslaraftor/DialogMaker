namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpFieldInfo : IDSharpMemberInfo
    {
        public IDSharpType FieldType { get; }
        public bool IsReadOnly { get; }
        public DSharpLiteralValue? RawValue { get; }
    }
}

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    public readonly struct LocalMemberInfo(LocalMemberType type, IDSharpParameterInfo value)
    {
        public LocalMemberType Type { get; } = type;
        public IDSharpParameterInfo Value { get; } = value;
    }
}

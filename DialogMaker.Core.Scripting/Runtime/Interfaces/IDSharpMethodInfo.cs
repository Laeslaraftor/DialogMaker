namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpMethodInfo : IDSharpMemberInfo
    {
        public IDSharpType? ReturnType { get; }

        public IDSharpParameterInfo[] GetParameters();
    }
}

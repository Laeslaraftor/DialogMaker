using DialogMaker.Core.Scripting.Runtime.Builders;

namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpMethodInfo : IDSharpMemberInfo
    {
        public IDSharpType? ReturnType { get; }
        public bool IsVirtual { get; }
        public bool IsOverride { get; }
        public bool IsSealed { get; }

        public IDSharpParameterInfo[] GetParameters();
        public DSharpBytecodeBuilder CreateBytecodeBuilder();
    }
}

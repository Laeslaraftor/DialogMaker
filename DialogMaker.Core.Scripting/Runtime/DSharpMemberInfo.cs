using DialogMaker.Core.Scripting.Compiler.Ast;

namespace DialogMaker.Core.Scripting.Runtime
{
    public abstract class DSharpMemberInfo : IDSharpMemberInfo
    {
        public abstract DSharpAssembly Assembly { get; }
        public DSharpMetadataToken MetadataToken { get; }
        public abstract string Name { get; }
        public abstract string FullName { get; }
        public abstract DSharpType? DeclaringType { get; }
        public DSharpAccessModifier Access { get; }
        public bool IsStatic { get; }

        IDSharpType? IDSharpMemberInfo.DeclaringType => DeclaringType;
        IDSharpAssembly IDSharpMemberInfo.Assembly => Assembly;
    }
}

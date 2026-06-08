using DialogMaker.Core.Scripting.Compiler.Ast;

namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpMemberInfo
    {
        public IDSharpAssembly Assembly { get; }
        public string Name { get; }
        public DSharpMetadataToken MetadataToken { get; }
        public IDSharpType? DeclaringType { get; }
        public DSharpAccessModifier Access { get; }
        public bool IsStatic { get; }
    }
}

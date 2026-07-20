using DialogMaker.Core.Scripting.Compiler.Ast;

namespace DialogMaker.Core.Scripting.Runtime
{
    public abstract class DSharpPropertyInfo : DSharpMemberInfo, IDSharpPropertyInfo
    {
        public IDSharpType PropertyType => throw new NotImplementedException();
        public bool CanRead => throw new NotImplementedException();
        public bool CanWrite => throw new NotImplementedException();

        public IDSharpMethodInfo? Getter => throw new NotImplementedException();

        public IDSharpMethodInfo? Setter => throw new NotImplementedException();

        public bool IsVirtual => throw new NotImplementedException();

        public bool IsSealed => throw new NotImplementedException();

        public IDSharpPropertyInfo? OverrideProperty => throw new NotImplementedException();

        public bool IsAbstract => throw new NotImplementedException();

        public DSharpAccessModifier? GetterAccess => throw new NotImplementedException();

        public DSharpAccessModifier? SetterAccess => throw new NotImplementedException();

        public IDSharpPropertyInfo[] GetImplementedProperties()
        {
            throw new NotImplementedException();
        }
    }
}

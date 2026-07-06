using DialogMaker.Core.Scripting.Compiler.Builders;

namespace DialogMaker.Core.Scripting.Runtime
{
    public abstract class DSharpMethodInfo : DSharpMemberInfo, IDSharpMethodInfo
    {
        public IDSharpType? ReturnType => throw new NotImplementedException();

        public bool IsVirtual => throw new NotImplementedException();

        public bool IsSealed => throw new NotImplementedException();

        public DSharpMethodType MethodType => throw new NotImplementedException();

        public IDSharpMethodInfo? OverrideMethod => throw new NotImplementedException();

        public bool IsAbstract => throw new NotImplementedException();

        public bool IsExtern => throw new NotImplementedException();

        public void CopyBytecodeTo(DSharpBytecodeBuilder builder)
        {
            throw new NotImplementedException();
        }

        public IDSharpType[] GetGenericParameters()
        {
            throw new NotImplementedException();
        }

        public IDSharpParameterInfo[] GetParameters()
        {
            throw new NotImplementedException();
        }
    }
}

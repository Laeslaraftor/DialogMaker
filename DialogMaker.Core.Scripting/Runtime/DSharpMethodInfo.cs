namespace DialogMaker.Core.Scripting.Runtime
{
    public abstract class DSharpMethodInfo : DSharpMemberInfo, IDSharpMethodInfo
    {
        public IDSharpType? ReturnType => throw new NotImplementedException();

        public bool IsVirtual => throw new NotImplementedException();

        public bool IsOverride => throw new NotImplementedException();

        public bool IsSealed => throw new NotImplementedException();

        public DSharpMethodType MethodType => throw new NotImplementedException();

        public IDSharpParameterInfo[] GetParameters()
        {
            throw new NotImplementedException();
        }
    }
}

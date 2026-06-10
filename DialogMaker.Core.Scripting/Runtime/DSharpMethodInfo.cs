namespace DialogMaker.Core.Scripting.Runtime
{
    public abstract class DSharpMethodInfo : DSharpMemberInfo, IDSharpMethodInfo
    {
        public IDSharpType? ReturnType => throw new NotImplementedException();

        public IDSharpParameterInfo[] GetParameters()
        {
            throw new NotImplementedException();
        }
    }
}

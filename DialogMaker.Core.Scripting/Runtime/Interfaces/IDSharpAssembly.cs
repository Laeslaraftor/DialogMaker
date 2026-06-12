namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpAssembly
    {
        public IDSharpFieldInfo[] GetGlobalVariables();
        public IDSharpMethodInfo[] GetGlobalFunctions();

        public IDSharpMemberInfo GetType(DSharpMetadataToken metadataToken);
    }
}

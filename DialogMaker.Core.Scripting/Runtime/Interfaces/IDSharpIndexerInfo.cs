namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpIndexerInfo : IDSharpPropertyInfo
    {
        public IDSharpParameterInfo[] GetParameters();
    }
}

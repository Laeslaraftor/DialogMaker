namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpParameterInfo
    {
        public string Name { get; }
        public IDSharpType Type { get; }
    }
}

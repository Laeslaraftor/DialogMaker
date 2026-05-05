namespace DialogMaker.Core.Editor.Nodes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NodeOutputAttribute(string name) : NameAttribute(name)
    {
    }
}

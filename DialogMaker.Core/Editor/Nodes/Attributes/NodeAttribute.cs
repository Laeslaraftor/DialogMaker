namespace DialogMaker.Core.Editor.Nodes
{
    public sealed class NodeAttribute(Type nodeType) : Attribute
    {
        public Type NodeType { get; } = nodeType;
    }
}

using System;

namespace DialogMaker.Core.Editor.Nodes
{
    public interface INode
    {
        public Guid Id { get; }
        public DialogNodeType NodeType { get; }
        public IPortDataConverter DataConverter { get; }
    }
}

using System;

namespace DialogMaker.Core.Editor.Nodes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NodeOutputAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }
}

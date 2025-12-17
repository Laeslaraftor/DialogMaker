using System;

namespace DialogMaker.Core.Editor.Nodes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NodeInputAttribute(string name) : NameAttribute(name)
    {
    }
}

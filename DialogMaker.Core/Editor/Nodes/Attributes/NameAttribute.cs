using System;

namespace DialogMaker.Core.Editor.Nodes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public sealed class NameAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }
}

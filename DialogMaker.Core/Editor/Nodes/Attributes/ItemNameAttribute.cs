using System;

namespace DialogMaker.Core.Editor.Nodes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ItemNameAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }
}

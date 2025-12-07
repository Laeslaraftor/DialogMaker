using System;

namespace DialogMaker.Core.Editor.Nodes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class NameAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }
}

using System;

namespace DialogMaker.Core.Editor.Nodes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public class NameAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }
}

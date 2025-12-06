using System;

namespace DialogMaker.Core.Editor.Nodes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ReferenceAttribute(DialogResourceType type) : Attribute
    {
        public DialogResourceType Type { get; } = type;
    }
}

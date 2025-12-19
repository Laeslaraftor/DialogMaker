using System;

namespace DialogMaker.Core.Editor.Nodes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class TypeAttribute(Type type, object defaultValue) : Attribute
    {
        public Type Type { get; } = type;
        public object DefaultValue { get; } = defaultValue;
    }
}

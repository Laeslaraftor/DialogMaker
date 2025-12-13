using System;

namespace DialogMaker.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ResourceTypeAttribute(Type type) : Attribute
    {
        public Type Type { get; } = type;
        public bool IsDev { get; set; }
    }
}

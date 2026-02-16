using System;

namespace DialogMaker.Core.Editor.Nodes
{
    public interface IValuePort
    {
        public bool CanPresetValue { get; }
        public object Value { get; set; }
        public Type ReflectionValueType { get; }
        public AllowedObjectValues AllowedValues { get; }
        public DialogResourceType? ResourceType { get; }
    }
    public interface IValuePort<T> : IValuePort
    {
        public new T Value { get; set; }
    }
}

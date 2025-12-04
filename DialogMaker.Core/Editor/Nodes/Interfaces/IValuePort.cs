namespace DialogMaker.Core.Editor.Nodes
{
    public interface IValuePort
    {
        public bool CanPresetValue { get; }
        public object Value { get; set; }
    }
    public interface IValuePort<T> : IValuePort
    {
        public new T Value { get; set; }
    }
}

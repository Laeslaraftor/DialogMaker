namespace DialogMaker.Core.Editor
{
    public readonly struct ItemEventArgs(object item)
    {
        public object Item { get; } = item;
    }
    public readonly struct ItemEventArgs<T>(T item)
    {
        public T Item { get; } = item;
    }
}

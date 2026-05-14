namespace DialogMaker.Core.Executioning
{
    public readonly struct ItemEventArgs<T>(T item)
    {
        public T Item { get; } = item;
    }
}

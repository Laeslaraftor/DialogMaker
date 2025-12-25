using System;

namespace DialogMaker.Core.Editor
{
    public class ItemEventArgs(object item) : ItemEventArgs<object>(item)
    {
    }
    public class ItemEventArgs<T>(T item) : EventArgs
    {
        public T Item { get; } = item;
    }
}

namespace DialogMaker.Core.Editor
{
    public class ItemActionEventArgs<T> : EventArgs
    {
        public ItemActionEventArgs(ItemAction action, T item)
        {
            Action = action;
            Item = item;
        }
        public ItemActionEventArgs(T item, ItemAction action)
            : this(action, item)
        {
        }

        public ItemAction Action { get; }
        public T Item { get; }
    }
}

namespace DialogMaker.Lib.Controllers
{
    public class SelectionEventArgs(SelectionMode mode, object item) : SelectionEventArgs<object>(mode, item)
    {
    }
    public class SelectionEventArgs<T>(SelectionMode mode, T item) : EventArgs
    {
        public SelectionMode SelectionMode { get; } = mode;
        public bool IsSingle { get; } = mode == SelectionMode.Single;
        public T Item { get; } = item;
    }
}

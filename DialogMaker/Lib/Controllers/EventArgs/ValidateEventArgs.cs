namespace DialogMaker.Lib.Controllers
{
    public class ValidateEventArgs<T>(T item) : EventArgs
    {
        public T Item { get; } = item;
        public bool IsValid { get; set; }
    }
}

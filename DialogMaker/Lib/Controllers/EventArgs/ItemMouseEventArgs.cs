using System.Windows.Input;

namespace DialogMaker.Lib.Controllers
{
    public class ItemMouseEventArgs<T>(T item, MouseEventArgs mouse) : EventArgs
    {
        public T Item { get; } = item;
        public MouseEventArgs Mouse { get; } = mouse;
    }
}

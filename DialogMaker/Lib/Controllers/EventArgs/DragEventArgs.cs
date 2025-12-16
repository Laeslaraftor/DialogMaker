using System.Windows;
using System.Windows.Input;

namespace DialogMaker.Lib.Controllers
{
    public class DragEventArgs(UIElement element, MouseEventArgs mouse) : DragEventArgs<UIElement>(element, mouse)
    {
    }
    public class DragEventArgs<T>(T element, MouseEventArgs mouse) : EventArgs
    {
        public T Element { get; } = element;
        public MouseEventArgs Mouse { get; } = mouse;
    }
}

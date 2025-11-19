using System.Windows;
using System.Windows.Input;

namespace DialogMaker.Lib.Controllers
{
    public class DragEventArgs(UIElement element, MouseEventArgs mouse) : EventArgs
    {
        public UIElement Element { get; } = element;
        public MouseEventArgs Mouse { get; } = mouse;
    }
}

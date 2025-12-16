using System.Windows;
using System.Windows.Input;

namespace DialogMaker.Lib.Controllers
{
    public class MouseClickEventArgs(MouseButtonEventArgs mouse, UIElement element) : EventArgs
    {
        public MouseButtonEventArgs Mouse { get; } = mouse;
        public UIElement Element { get; } = element;
    }
}

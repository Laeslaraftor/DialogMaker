using System.Windows;
using System.Windows.Input;

namespace DialogMaker.Lib.Controllers
{
    public class DragCheckEventArgs(DependencyObject potentialDragObject, MouseEventArgs mouse) : EventArgs
    {
        public DependencyObject PotentialDragObject { get; set; } = potentialDragObject;
        public MouseEventArgs Mouse { get; } = mouse;
        public bool Ignore { get; set; }
    }
}

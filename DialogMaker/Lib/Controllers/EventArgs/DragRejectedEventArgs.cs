using System.Windows.Input;

namespace DialogMaker.Lib.Controllers
{
    public class DragRejectedEventArgs(MouseButtonEventArgs mouse, DragRejectReason reason) : EventArgs
    {
        public MouseButtonEventArgs Mouse { get; } = mouse;
        public DragRejectReason Reason { get; } = reason;
    }
}

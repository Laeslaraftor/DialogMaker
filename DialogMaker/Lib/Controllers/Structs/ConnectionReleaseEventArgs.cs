using DialogMaker.Editor;
using System.Windows.Input;

namespace DialogMaker.Lib.Controllers
{
    public struct ConnectionReleaseEventArgs(MouseEventArgs mouse, DialogProjectNodePortProxy port)
    {
        public MouseEventArgs Mouse { get; } = mouse;
        public DialogProjectNodePortProxy Port { get; } = port;
    }
}

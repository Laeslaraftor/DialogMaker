using DialogMaker.Core;
using DialogMaker.Editor;
using System.Windows.Media;
using System.Windows;

namespace DialogMaker.Lib.Controllers
{
    public interface IReferenceView
    {
        public bool CanSetReference { get; }
        public string Placeholder { get; set; }
        public ProjectResourceItem? Item { get; set; }
        public DialogResourceType? RequestedResourceType { get; set; }
        public FrameworkElement View { get; }

        public Point GetPosition(Visual relativeTo);
    }
}

using System.Windows;
using System.Windows.Media;

namespace DialogMaker.Lib
{
    public interface ISelectable
    {
        public bool IsSelected { get; set; }
        public int GroupCount { get; }
        public FrameworkElement? View { get; }

        public Rect GetViewRect(Visual container);
        public IEnumerable<ISelectable> GetOtherSelectables();
    }
}

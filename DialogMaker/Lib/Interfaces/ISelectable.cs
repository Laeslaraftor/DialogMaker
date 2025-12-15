using System.Windows;

namespace DialogMaker.Lib
{
    public interface ISelectable
    {
        public bool IsSelected { get; set; }
        public FrameworkElement? View { get; }

        public IEnumerable<ISelectable> GetOtherSelectables();
    }
}

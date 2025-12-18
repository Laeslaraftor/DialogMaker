using System.ComponentModel;
using System.Windows.Controls;

namespace DialogMaker.Editor.Menus
{
    public interface IContextMenuModifier : INotifyPropertyChanged, IDisposable
    {
        public void Modify(ContextMenu menu, ItemCollection items);
    }
}

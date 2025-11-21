using System.Windows.Controls;

namespace DialogMaker.Editor.Menus
{
    public interface IContextMenuModifier
    {
        public void Modify(ItemCollection menu);
    }
}

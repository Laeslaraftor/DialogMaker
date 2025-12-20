using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace DialogMaker.Editor.Menus
{
    public class ContextMenuContainer(string name, IEnumerable<IContextMenuModifier> modifiers) 
        : ContextMenuItemModifier(name), IContextMenuModifier
    {
        public ContextMenuContainer(string name, IContextMenuModifier modifier)
            : this(name, new ReadOnlyCollection<IContextMenuModifier>([modifier]))
        {
        }
        public ContextMenuContainer(string icon, string name, IEnumerable<IContextMenuModifier> modifiers)
            : this(name, modifiers)
        {
            Icon = icon;
        }
        public ContextMenuContainer(string icon, string name, IContextMenuModifier modifier)
            : this(icon, name, new ReadOnlyCollection<IContextMenuModifier>([modifier]))
        {
        }

        public IEnumerable<IContextMenuModifier> Modifiers { get; } = modifiers;

        #region Управление

        public void Modify(ContextMenu menu, ItemCollection items)
        {
            var item = GetItem(menu, items);

            foreach (var modifier in Modifiers)
            {
                modifier.Modify(menu, item.Items);
            }

            items.Add(item);
        }

        #endregion
    }
}

using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace DialogMaker.Editor.Menus
{
    public class ContextMenuContainer(string name, IEnumerable<IContextMenuModifier> modifiers)
        : ContextMenuAction(name, (Action<object?>?)null), IContextMenuModifier
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
            : this(icon, name, [modifier])
        {
        }
        public ContextMenuContainer(string icon, string name, Action<object?>? execute, Func<object?, bool>? canExecute, IEnumerable<IContextMenuModifier> modifiers)
            : this(icon, name, modifiers)
        {
            CanExecute = canExecute;
            Execute = execute;
        }

        public IEnumerable<IContextMenuModifier> Modifiers { get; } = modifiers;

        #region Управление

        protected override void Modify(ContextMenu menu, MenuItem item)
        {
            base.Modify(menu, item);

            foreach (var modifier in Modifiers)
            {
                modifier.Modify(menu, item.Items);
            }
        }

        #endregion
    }
}

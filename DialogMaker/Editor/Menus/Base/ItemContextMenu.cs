using System.Windows.Controls;

namespace DialogMaker.Editor.Menus
{
    public abstract class ItemContextMenu
    {
        public ContextMenu ContextMenu
        {
            get
            {
                if (_contextMenu == null)
                {
                    _contextMenu = new();
                    UpdateMenu();
                }

                return _contextMenu;
            }
        }

        private ContextMenu? _contextMenu;

        #region Операторы

        public static implicit operator ContextMenu(ItemContextMenu menu)
        {
            return menu.ContextMenu;
        }

        #endregion

        #region Управление

        public void UpdateMenu()
        {
            if (_contextMenu == null)
            {
                _contextMenu = ContextMenu;
                return;
            }

            ContextMenu.Items.Clear();

            foreach (var modifier in GetItems())
            {
                modifier.Modify(ContextMenu.Items);
            }
        }

        protected abstract IEnumerable<IContextMenuModifier> GetItems();

        #endregion
    }
}

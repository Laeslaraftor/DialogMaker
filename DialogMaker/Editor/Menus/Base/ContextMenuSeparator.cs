using System.Windows.Controls;

namespace DialogMaker.Editor.Menus
{
    public class ContextMenuSeparator : IContextMenuModifier
    {
        public void Modify(ItemCollection menu)
        {
            menu.Add(new Separator());
        }

        #region Статика

        public static readonly ContextMenuSeparator Instance = new();

        #endregion
    }
}

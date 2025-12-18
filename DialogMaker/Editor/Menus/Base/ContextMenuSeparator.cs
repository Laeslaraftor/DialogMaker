using DialogMaker.Core;
using System.Windows.Controls;

namespace DialogMaker.Editor.Menus
{
    public class ContextMenuSeparator : Disposable, IContextMenuModifier
    {
        public void Modify(ContextMenu menu, ItemCollection items)
        {
            items.Add(new Separator());
        }

        #region Статика

        public static readonly ContextMenuSeparator Instance = new();

        #endregion
    }
}

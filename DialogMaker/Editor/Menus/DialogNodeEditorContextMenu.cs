using DialogMaker.Core;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Lib;

namespace DialogMaker.Editor.Menus
{
    public class DialogNodeEditorContextMenu : TypeContextMenu<DialogProjectNode>
    {
        public DialogNodeEditorContextMenu()
        {
        }
        public DialogNodeEditorContextMenu(DialogProjectNode item) : base(item)
        {
        }

        protected override IEnumerable<IContextMenuModifier> GetItems()
        {
            yield return new ContextMenuAction("Копировать",
                CanClipboardAction, RemoveDialog, Icons.Copy);
            yield return new ContextMenuAction("Вырезать",
                CanClipboardAction, RemoveDialog, Icons.Cut);
            yield return new ContextMenuAction("Вставить",
                CanClipboardAction, RemoveDialog, Icons.Paste);
            yield return ContextMenuSeparator.Instance;
            yield return new ContextMenuAction("Удалить",
                CanDelete, RemoveDialog, Icons.Delete);
        }

        #region Команды

        private bool CanClipboardAction(object? parameter)
        {
            return false;
        }
        private bool CanDelete(object? parameter)
        {
            return Resolve(parameter, node =>
            {
                return node.Dialog.SelectedNodes.Count > 0;
            });
        }

        private void RemoveDialog(object? parameter)
        {
            Resolve(parameter, node =>
            {
                node.Dialog.RemoveSelectedNodes(); 
            });
        }

        #endregion

        #region Статика

        public static readonly DialogNodeEditorContextMenu Instance = new();

        #endregion
    }
}

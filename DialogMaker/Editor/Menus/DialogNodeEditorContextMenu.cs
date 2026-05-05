using DialogMaker.Lib;
using System.Windows.Input;

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
            foreach (var modifier in DialogEditorContextMenu.CreateClipboardModifiers(Item?.Dialog))
            {
                yield return modifier;
            }

            yield return new ContextMenuAction("Удалить",
                CanDelete, RemoveDialog, Icons.Delete)
            {
                Shortcut = Key.Delete.ToString()
            };
        }

        #region Команды

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

using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Lib;
using DialogMaker.Core;

namespace DialogMaker.Editor.Menus
{
    public class DialogEditorContextMenu : TypeContextMenu<ProjectDialog>
    {
        public DialogEditorContextMenu()
        {
        }
        public DialogEditorContextMenu(ProjectDialog item) : base(item)
        {
        }

        protected override IEnumerable<IContextMenuModifier> GetItems()
        {
            yield return new ContextMenuContainer("Добавить", GetNodes());
            yield return ContextMenuSeparator.Instance;
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

        private IEnumerable<IContextMenuModifier> GetNodes()
        {
            foreach (var nodeInfo in DialogProjectDialogNode.AvailableNodes)
            {
                var name = nodeInfo.Key.GetEnumAttribute<NameAttribute>()?.Name;
                name ??= nodeInfo.Key.ToString();

                yield return new ContextMenuAction(name, p => AddNode(p, nodeInfo.Key));
            }
        }

        #region Команды

        private void AddNode(object? parameter, DialogNodeType nodeType)
        {
            Resolve(parameter, dialog =>
            {
                try
                {
                    var node = dialog.Original.CreateNode(nodeType);
                    var position = dialog.LastMouseClickPosition;
                    node.Position = new((float)position.X, (float)position.Y);
                }
                catch (Exception error)
                {
                    error.Alert();
                }
            });
        }

        private bool CanClipboardAction(object? parameter)
        {
            return false;
        }
        private bool CanDelete(object? parameter)
        {
            return Resolve(parameter, dialog =>
            {
                return dialog.SelectedNodes.Count > 0;
            });
        }

        private void RemoveDialog(object? parameter)
        {
            Resolve(parameter, dialog =>
            {
                dialog.RemoveSelectedNodes();
            });
        }

        #endregion

        #region Статика

        public static readonly DialogContextMenu Instance = new();

        #endregion
    }
}

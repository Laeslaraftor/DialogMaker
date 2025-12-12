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
            yield return new ContextMenuAction("Удалить",
                CanExecute, RemoveDialog, Icons.Delete);
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

        private void RemoveDialog(object? parameter)
        {
            
        }

        #endregion

        #region Статика

        public static readonly DialogContextMenu Instance = new();

        #endregion
    }
}

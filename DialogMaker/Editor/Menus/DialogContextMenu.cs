using DialogMaker.Core.Editor;
using DialogMaker.Lib;
using System.Diagnostics;

namespace DialogMaker.Editor.Menus
{
    public class DialogContextMenu : TypeContextMenu<DialogProjectDialog>
    {
        public DialogContextMenu()
        {
        }
        public DialogContextMenu(DialogProjectDialog item) : base(item)
        {
        }

        protected override IEnumerable<IContextMenuModifier> GetItems()
        {
            yield return new ContextMenuAction("Открыть в проводнике",
                CanExecute, OpenInExplorer, Icons.OpenFolder);
            yield return ContextMenuSeparator.Instance;
            yield return new ContextMenuAction("Удалить",
                CanExecute, RemoveDialog, Icons.Delete);
        }

        #region Команды

        #region Команды

        private void OpenInExplorer(object? parameter)
        {
            Resolve(parameter, dialog =>
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = dialog.Folder,
                    UseShellExecute = true
                });
            });
        }
        private void RemoveDialog(object? parameter)
        {
            Resolve(parameter, dialog =>
            {
                dialog.Pack.RemoveDialog(dialog);
            });
        }

        #endregion

        #endregion

        #region Статика

        public static readonly DialogContextMenu Instance = new();

        #endregion
    }
}

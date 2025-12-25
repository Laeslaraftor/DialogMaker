using DialogMaker.Lib;
using System.Diagnostics;

namespace DialogMaker.Editor.Menus
{
    public class FileContextMenu : TypeContextMenu<ProjectFile>
    {
        public FileContextMenu()
        {
        }
        public FileContextMenu(ProjectFile item) : base(item)
        {
            _moveActions = new(item);
        }

        private readonly MoveResourceItemActions? _moveActions;

        protected override IEnumerable<IContextMenuModifier> GetItems()
        {
            yield return new ContextMenuAction("Открыть",
                CanExecute, Open, Icons.OpenFile);
            yield return new ContextMenuAction("Показать в проводнике",
                CanExecute, OpenInExplorer, Icons.OpenFolder);

            if (_moveActions != null)
            {
                yield return _moveActions.GetModifier();
            }

            yield return ContextMenuSeparator.Instance;
            yield return new ContextMenuAction("Удалить",
                CanExecute, RemoveFile, Icons.Delete);
        }


        #region Команды

        private void Open(object? parameter)
        {
            Resolve(parameter, file =>
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = file.FilePath,
                    UseShellExecute = true
                });
            });
        }
        private void OpenInExplorer(object? parameter)
        {
            Resolve(parameter, file =>
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select, \"{file.FilePath}\"",
                    UseShellExecute = true
                });
            });
        }
        private void RemoveFile(object? parameter)
        {
            Resolve(parameter, file =>
            {
                file.Original.Resources.RemoveItem(file.Original);
            });
        }

        #endregion

        #region Статика

        public static readonly FileContextMenu Instance = new();

        #endregion
    }
}

using DialogMaker.Core.Editor;
using DialogMaker.Editor.Data;
using DialogMaker.Lib;
using System.Diagnostics;
using static DialogMaker.LibExtensions;

namespace DialogMaker.Editor.Menus
{
    public class DialogPackContextMenu : TypeContextMenu<DialogProjectPack>
    {
        public DialogPackContextMenu()
        {
        }
        public DialogPackContextMenu(DialogProjectPack item) : base(item)
        {
        }


        protected override IEnumerable<IContextMenuModifier> GetItems()
        {
            yield return new ContextMenuAction("Открыть в проводнике",
                CanExecute, OpenInExplorer, Icons.OpenFolder);
            yield return new ContextMenuAction("Создать диалог", 
                CanExecute, CreateDialog, Icons.Create);
            yield return ContextMenuSeparator.Instance;
            yield return new ContextMenuAction("Удалить",
                CanExecute, RemovePack, Icons.Delete);
        }

        #region Команды

        private void OpenInExplorer(object? parameter)
        {
            Resolve(parameter, pack =>
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = pack.Folder,
                    UseShellExecute = true
                });
            });
        }
        private void CreateDialog(object? parameter)
        {
            Resolve(parameter, async pack =>
            {
                var info = await ProjectItemCreationInfo.Create("Создать диалог", "диалог");

                if (info == null)
                {
                    return;
                }

                Try(() => pack.CreateDialog(info.Id, info.Name));
            });
        }

        private void RemovePack(object? parameter)
        {
            Resolve(parameter, pack =>
            {
                pack.Project.RemovePack(pack);
            });            
        }

        #endregion

        #region Статика

        public static readonly DialogPackContextMenu Instance = new();

        #endregion
    }
}

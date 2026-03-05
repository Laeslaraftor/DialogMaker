using DialogMaker.Core;
using DialogMaker.Lib;

namespace DialogMaker.Editor.Menus
{
    public class MoveResourceItemActions(ProjectResourceItem item)
    {
        private ProjectResources? CurrentResources => _item.Project.LastShowedTabItem?.Resources;

        private readonly ProjectResourceItem _item = item;
        private ContextMenuContainer? _container;

        #region Управление

        public IContextMenuModifier GetModifier()
        {
            _container ??= new ContextMenuContainer(Icons.RightArrow, "Переместить", FolderContent());
            return _container;
        }

        private IEnumerable<IContextMenuModifier> FolderContent()
        {
            yield return new ContextMenuAction("В проект", CanMoveToProject, MoveToProject);
            yield return new ContextMenuAction("В набор", CanMoveToPack, MoveToPack);
            yield return new ContextMenuAction("В диалог", CanMoveToDialog, MoveToDialog);
            //yield return new ContextMenuAction("В другое...", MoveToProject);
        }

        #endregion

        #region Команды

        private bool CanMoveTo(DialogResourcesFlags flags)
        {
            return _item.Model.Resources.Flags != flags;
        }

        private bool CanMoveToProject(object? _)
        {
            return CanMoveTo(DialogResourcesFlags.Root);
        }
        private bool CanMoveToPack(object? _)
        {
            if (CurrentResources == null ||
                !CanMoveTo(DialogResourcesFlags.Pack))
            {
                return false;
            }

            return CurrentResources.Flags.HasFlag(DialogResourcesFlags.Pack) ||
                   CurrentResources.Flags.HasFlag(DialogResourcesFlags.Dialog);
        }
        private bool CanMoveToDialog(object? _)
        {
            return CurrentResources?.Flags.HasFlag(DialogResourcesFlags.Dialog) == true &&
                   CanMoveTo(DialogResourcesFlags.Dialog);
        }

        private void MoveToProject(object? _)
        {
            if (CanMoveToProject(_))
            {
                MoveTo(_item.Project.Resources);
            }
        }
        private void MoveToPack(object? _)
        {
            if (CanMoveToPack(_) &&
                CurrentResources?.TryFindByFlags(DialogResourcesFlags.Pack, out var resources) == true)
            {
                MoveTo(resources);
            }
        }
        private void MoveToDialog(object? _)
        {
            if (CanMoveToDialog(_) &&
                CurrentResources?.TryFindByFlags(DialogResourcesFlags.Dialog, out var resources) == true)
            {
                MoveTo(resources);
            }
        }
        private void MoveToCustom(object? _)
        {
        }

        private void MoveTo(ProjectResources resources)
        {
            try
            {
                _item.Model.MoveTo(resources.Original);
            }
            catch (Exception error)
            {
                error.Log();
            }
        }

        #endregion
    }
}

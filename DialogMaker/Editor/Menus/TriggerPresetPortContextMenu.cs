using DialogMaker.Lib;

namespace DialogMaker.Editor.Menus
{
    public class TriggerPresetPortContextMenu(ProjectTriggerPresetPort port) : ItemContextMenu
    {
        private readonly ProjectTriggerPresetPort _port = port;

        protected override IEnumerable<IContextMenuModifier> GetItems()
        {
            yield return new ContextMenuAction("Удалить",
                CanRemove, Remove, Icons.Delete);
        }

        #region Команды

        private bool CanRemove(object? parameter)
        {
            return _port.TriggerPreset.Contains(_port);
        }
        private void Remove(object? parameter)
        {
            _port.TriggerPreset.Remove(_port);
        }

        #endregion
    }
}

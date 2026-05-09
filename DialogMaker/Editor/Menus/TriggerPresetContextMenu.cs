using DialogMaker.Lib;

namespace DialogMaker.Editor.Menus
{
    public class TriggerPresetContextMenu : TypeContextMenu<ProjectTriggerPreset>
    {
        public TriggerPresetContextMenu()
        {
        }
        public TriggerPresetContextMenu(ProjectTriggerPreset item) : base(item)
        {
            _moveActions = new(item);
        }

        private readonly MoveResourceItemActions? _moveActions;

        protected override IEnumerable<IContextMenuModifier> GetItems()
        {
            if (Item != null)
            {
                yield return new ContextMenuAction("Добавить входной параметр",
                    Item.CreateInputCommand, Icons.Import);
                yield return new ContextMenuAction("Добавить выходной параметр",
                    Item.CreateOutputCommand, Icons.Export);
            }

            if (_moveActions != null)
            {
                yield return _moveActions.GetModifier();
            }

            yield return ContextMenuSeparator.Instance;
            yield return new ContextMenuAction("Удалить",
                CanExecute, RemovePreset, Icons.Delete);
        }

        #region Команды

        private void RemovePreset(object? parameter)
        {
            Resolve(parameter, preset =>
            {
                preset.Original.Resources.RemoveTriggerPreset(preset.Original);
            });
        }

        #endregion

        #region Статика

        public static readonly TriggerPresetContextMenu Instance = new();

        #endregion
    }
}

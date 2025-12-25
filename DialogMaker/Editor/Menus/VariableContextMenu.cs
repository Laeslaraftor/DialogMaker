using DialogMaker.Lib;

namespace DialogMaker.Editor.Menus
{
    public class VariableContextMenu : TypeContextMenu<ProjectVariable>
    {
        public VariableContextMenu()
        {
        }
        public VariableContextMenu(ProjectVariable item) : base(item)
        {
            _moveActions = new(item);
        }

        private readonly MoveResourceItemActions? _moveActions;

        protected override IEnumerable<IContextMenuModifier> GetItems()
        {
            if (_moveActions != null)
            {
                yield return _moveActions.GetModifier();
                yield return ContextMenuSeparator.Instance;
            }

            yield return new ContextMenuAction("Удалить переменную",
                CanExecute, RemoveString, Icons.Delete);
        }

        #region Команды

        private void RemoveString(object? parameter)
        {
            Resolve(parameter, variable =>
            {
                variable.Original.Resources.RemoveVariable(variable.Original);
            });
        }

        #endregion

        #region Статика

        public static readonly VariableContextMenu Instance = new();

        #endregion
    }
}

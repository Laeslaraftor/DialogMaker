using DialogMaker.Lib;

namespace DialogMaker.Editor.Menus
{
    public class CharacterContextMenu : TypeContextMenu<ProjectCharacter>
    {
        public CharacterContextMenu()
        {
        }
        public CharacterContextMenu(ProjectCharacter item) : base(item)
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

            yield return new ContextMenuAction("Удалить",
                CanExecute, RemoveCharacter, Icons.Delete);
        }

        #region Команды

        private void RemoveCharacter(object? parameter)
        {
            Resolve(parameter, character =>
            {
                character.Original.Resources.Characters.Remove(character.Original);
            });
        }

        #endregion

        #region Статика

        public static readonly CharacterContextMenu Instance = new();

        #endregion
    }
}

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
        }

        protected override IEnumerable<IContextMenuModifier> GetItems()
        {
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

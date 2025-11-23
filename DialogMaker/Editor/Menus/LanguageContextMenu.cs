using DialogMaker.Lib;
using System.Diagnostics;

namespace DialogMaker.Editor.Menus
{
    public class LanguageContextMenu : TypeContextMenu<ProjectLanguage>
    {
        public LanguageContextMenu()
        {
        }
        public LanguageContextMenu(ProjectLanguage item) : base(item)
        {
        }

        protected override IEnumerable<IContextMenuModifier> GetItems()
        {
            yield return new ContextMenuAction("Установить по умолчанию",
                CanSetAsDefault, SetAsDefault, Icons.Language);
            yield return ContextMenuSeparator.Instance;
            yield return new ContextMenuAction("Удалить",
                CanExecute, RemoveLanguage, Icons.Delete);
        }

        #region Команды

        private bool CanSetAsDefault(object? parameter)
        {
            bool result = false;
            bool resolved = Resolve(parameter, language =>
            {
                result = language.Project?.DefaultLanguage == null ||
                         language.Project?.DefaultLanguage != language.Language;
            });

            if (resolved)
            {
                return result;
            }

            return false;
        }
        private void SetAsDefault(object? parameter)
        {
            Resolve(parameter, language =>
            {
                language.Project?.DefaultLanguage = language.Language;
            });
        }

        private void RemoveLanguage(object? parameter)
        {
            Resolve(parameter, language =>
            {
                language.Controller.Languages.Remove(language);
            });
        }

        #endregion

        #region Статика

        public static readonly LanguageContextMenu Instance = new();

        #endregion
    }
}

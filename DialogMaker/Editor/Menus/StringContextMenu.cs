using DialogMaker.Lib;

namespace DialogMaker.Editor.Menus
{
    public class StringContextMenu : TypeContextMenu<ProjectString>
    {
        public StringContextMenu()
        {
        }
        public StringContextMenu(ProjectString item) : base(item)
        {
        }

        protected override IEnumerable<IContextMenuModifier> GetItems()
        {
            yield return new ContextMenuAction("Добавить вариант",
                CanAddVariant, AddVariant, Icons.Add);
            yield return ContextMenuSeparator.Instance;
            yield return new ContextMenuAction("Удалить строку",
                CanExecute, RemoveString, Icons.Delete);
        }

        #region Команды

        private bool CanAddVariant(object? parameter)
        {
            bool result = false;
            bool resolved = Resolve(parameter, str =>
            {
                result = str.AddVariantCommand.CanExecute(null);
            });

            if (resolved)
            {
                return result;
            }

            return false;
        }
        private void AddVariant(object? parameter)
        {
            Resolve(parameter, str =>
            {
                str.AddVariantCommand.Execute(null);
            });
        }

        private void RemoveString(object? parameter)
        {
            Resolve(parameter, str =>
            {
                str.Original.Resources.RemoveString(str.Original);
            });
        }

        #endregion

        #region Статика

        public static readonly StringContextMenu Instance = new();

        #endregion
    }
}

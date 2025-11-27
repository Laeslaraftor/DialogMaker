using DialogMaker.Lib;

namespace DialogMaker.Editor.Menus
{
    public class StringVariantContextMenu : TypeContextMenu<ProjectStringVariant>
    {
        public StringVariantContextMenu()
        {
        }
        public StringVariantContextMenu(ProjectStringVariant item) : base(item)
        {
        }

        protected override IEnumerable<IContextMenuModifier> GetItems()
        {
            yield return new ContextMenuAction("Добавить вариант",
                CanAddVariant, AddVariant, Icons.Add);
            yield return ContextMenuSeparator.Instance;
            yield return new ContextMenuAction("Удалить вариант",
                CanExecute, RemoveVariant, Icons.Delete);
            yield return new ContextMenuAction("Удалить строку",
                CanExecute, RemoveString, Icons.Delete);
        }

        #region Команды

        private bool CanAddVariant(object? parameter)
        {
            bool result = false;
            bool resolved = Resolve(parameter, str =>
            {
                result = str.String.AddVariantCommand.CanExecute(null);
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
                str.String.AddVariantCommand.Execute(null);
            });
        }

        private void RemoveVariant(object? parameter)
        {
            Resolve(parameter, str =>
            {
                str.String.Original.Remove(str.Original);
            });
        }
        private void RemoveString(object? parameter)
        {
            Resolve(parameter, str =>
            {
                str.String.Original.Resources.RemoveString(str.String.Original);
            });
        }

        #endregion

        #region Статика

        public static readonly StringVariantContextMenu Instance = new();

        #endregion
    }
}

namespace DialogMaker.Editor.Menus
{
    public abstract class TypeContextMenu<T> : ItemContextMenu
    {
        public TypeContextMenu()
        {
        }
        public TypeContextMenu(T item)
        {
            Item = item;
        }

        protected readonly T? Item;

        #region Управление

        protected virtual bool CanExecute(object? parameter)
        {
            return Resolve(parameter, p => { });
        }

        protected bool Resolve(object? parameter, Action<T> execute)
        {
            if (Item != null)
            {
                execute(Item);
            }
            else if (parameter is T typedParameter)
            {
                execute(typedParameter);
            }
            else
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}

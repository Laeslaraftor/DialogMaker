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
            return Resolve(parameter, obj =>
            {
                execute(obj);
                return true;
            });
        }
        protected bool Resolve(object? parameter, Func<T, bool> execute)
        {
            if (Item != null)
            {
                return execute(Item);
            }
            else if (parameter is T typedParameter)
            {
                return execute(typedParameter);
            }

            return false;
        }

        #endregion
    }
}

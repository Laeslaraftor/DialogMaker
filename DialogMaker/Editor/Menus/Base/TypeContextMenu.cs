namespace DialogMaker.Editor.Menus
{
    public abstract class TypeContextMenu<T> : ItemContextMenu
    {
        public TypeContextMenu()
        {
        }
        public TypeContextMenu(T item)
        {
            _item = item;
        }

        private readonly T? _item;

        #region Управление

        protected bool CanExecute(object? parameter)
        {
            return Resolve(parameter, p => { });
        }

        protected bool Resolve(object? parameter, Action<T> execute)
        {
            if (_item != null)
            {
                execute(_item);
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

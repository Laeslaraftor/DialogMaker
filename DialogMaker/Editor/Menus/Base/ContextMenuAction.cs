using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Editor.Menus
{
    public class ContextMenuAction : ContextMenuItemModifier, IContextMenuModifier, ICommand
    {
        public ContextMenuAction(string name, Action<object?>? execute, string? icon = null)
            : this(name, execute, null, icon)
        {
        }
        public ContextMenuAction(string name, ICommand command, string? icon = null)
            : this(name, command.CanExecute, command.Execute, icon)
        {
        }
        public ContextMenuAction(string name, Func<object?, bool>? canExecute, Action<object?> execute, string? icon = null)
            : this(name, execute, canExecute, icon)
        {
        }
        public ContextMenuAction(string name, Action<object?>? execute, Func<object?, bool>? canExecute, string? icon = null)
            : base(icon, name)
        {
            Icon = icon;
            Name = name;
            Execute = execute;
            CanExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public Action<object?>? Execute { get; protected set; }
        public Func<object?, bool>? CanExecute { get; protected set; }
        public object? CommandParameter
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(CommandParameter));
                    field = value;
                    OnPropertyChanged(nameof(CommandParameter));
                }
            }
        }

        #region Управление

        public void Modify(ContextMenu menu, ItemCollection items)
        {
            var item = GetItem(menu, items);
            Modify(menu, item);

            items.Add(item);
        }

        protected virtual void Modify(ContextMenu menu, MenuItem item)
        {
        }

        bool ICommand.CanExecute(object? parameter)
        {
            if (CanExecute == null)
            {
                return true;
            }

            return CanExecute(parameter);
        }
        void ICommand.Execute(object? parameter)
        {
            Execute?.Invoke(parameter);
        }

        protected override void SetupItem(MenuItem item)
        {
            base.SetupItem(item);
            item.Command = this;
            item.CommandParameter = CommandParameter;
        }

        #endregion

        #region События

        protected override void OnContextMenuOpened(ContextMenu menu, ItemCollection items, MenuItem item)
        {
            base.OnContextMenuOpened(menu, items, item);

            if (CanExecute != null)
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}

using DialogMaker.Lib;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Editor.Menus
{
    public class ContextMenuAction : ContextMenuItemModifier, IContextMenuModifier, ICommand
    {
        public ContextMenuAction(string name, Action<object?> execute, string? icon = null)
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
        public ContextMenuAction(string name, Action<object?> execute, Func<object?, bool>? canExecute, string? icon = null)
            : base(icon, name)
        {
            Icon = icon;
            Name = name;
            Execute = execute;
            CanExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public Action<object?> Execute { get; }
        public Func<object?, bool>? CanExecute { get; }

        #region Управление

        public void Modify(ContextMenu menu, ItemCollection items)
        {
            var item = GetItem(menu, items);
            items.Add(item);
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
            Execute(parameter);
        }

        protected override void SetupItem(MenuItem item)
        {
            base.SetupItem(item);
            item.Command = this;
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

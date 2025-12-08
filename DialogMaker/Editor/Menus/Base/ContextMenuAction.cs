using DialogMaker.Lib;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Editor.Menus
{
    public class ContextMenuAction : IContextMenuModifier, ICommand
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
        {
            Icon = icon;
            Name = name;
            Execute = execute;
            CanExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public string? Icon { get; }
        public string Name
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;

                    foreach (var item in _items)
                    {
                        item.Header = value;
                    }
                }
            }
        }
        public Action<object?> Execute { get; }
        public Func<object?, bool>? CanExecute { get; }

        private readonly List<MenuItem> _items = [];

        #region Управление

        public void Modify(ItemCollection menu)
        {
            MenuItem item = new()
            {
                Header = Name,
                Command = this,
            };

            if (Icons.TryCreateIconBlock(Icon, out var iconBlock))
            {
                iconBlock.FontSize = 14;
            }

            item.Icon = iconBlock;

            menu.Add(item);
            _items.Add(item);
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

        #endregion
    }
}

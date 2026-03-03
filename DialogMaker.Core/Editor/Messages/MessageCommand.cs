using System;
using System.Windows.Input;

namespace DialogMaker.Core.Editor.Messages
{
    public class MessageCommand : ObservableObject, ICommand
    {
        public MessageCommand(string name, Action<object?> handler)
            : this(name, handler, true)
        {
        }
        private MessageCommand(string name, Action<object?> handler, bool readOnly)
        {
            Name = name;

            _handler = handler;
            _isReadOnly = readOnly;
        }

        public event EventHandler? CanExecuteChanged;

        public string Name
        {
            get => field;
            private set
            {
                if (!_isReadOnly && field != value)
                {
                    InvokePropertyChanging(nameof(Name));
                    field = value;
                    InvokePropertyChanged(nameof(Name));
                }
            }
        }
        public string? Description
        {
            get => field;
            private set
            {
                if (!_isReadOnly && field != value)
                {
                    InvokePropertyChanging(nameof(Description));
                    field = value;
                    InvokePropertyChanged(nameof(Description));
                }
            }
        }
        public bool IsEnabled
        {
            get => field;
            private set
            {
                if (!_isReadOnly && field != value)
                {
                    InvokePropertyChanging(nameof(IsEnabled));
                    field = value;
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                    InvokePropertyChanged(nameof(IsEnabled));
                }
            }
        }

        private readonly Action<object?> _handler;
        private readonly bool _isReadOnly;

        #region Управление

        public void Execute(object? parameter)
        {
            _handler(parameter);
        }
        public bool CanExecute(object? parameter) => IsEnabled;

        #endregion

        #region Статика

        public static Token Create(string name, Action<object?> handler)
        {
            return Create(name, handler);
        }
        public static Token Create(string name, string? description, Action<object?> handler)
        {
            return new(new(name, handler, false)
            {
                Description = description
            });
        }

        #endregion

        #region Классы

        public readonly struct Token(MessageCommand command)
        {
            public MessageCommand Command { get; } = command;
            public string Name
            {
                get => Command.Name;
                set => Command.Name = value;
            }
            public string? Description
            {
                get => Command.Description;
                set => Command.Description = value;
            }
            public bool IsEnabled
            {
                get => Command.IsEnabled;
                set => Command.IsEnabled = value;
            }

            public static implicit operator MessageCommand(Token token) => token.Command;
        }

        #endregion
    }
}

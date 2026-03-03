using Acly;
using System.Collections.Generic;

namespace DialogMaker.Core.Editor.Messages
{
    public class Message : ObservableObject
    {
        public Message(MessageImportance importance, string title, string text, IEnumerable<MessageCommand>? commands)
            : this(importance, title, text, commands, true)
        {
        }
        private Message(MessageImportance importance, string title, string text, IEnumerable<MessageCommand>? commands, bool readOnly)
        {
            Importance = importance;
            Title = title;
            Text = text;

            _isReadOnly = readOnly;
            _commands = commands == null ? [] : new(commands);
            Commands = new(_commands);
        }

        public MessageImportance Importance
        {
            get => field;
            private set
            {
                if (!_isReadOnly && field != value)
                {
                    InvokePropertyChanging(nameof(Importance));
                    field = value;
                    InvokePropertyChanged(nameof(Importance));
                }
            }
        }
        public string Title
        {
            get => field;
            private set
            {
                if (!_isReadOnly && field != value)
                {
                    InvokePropertyChanging(nameof(Title));
                    field = value;
                    InvokePropertyChanged(nameof(Title));
                }
            }
        }
        public string Text
        {
            get => field;
            private set
            {
                if (!_isReadOnly && field != value)
                {
                    InvokePropertyChanging(nameof(Text));
                    field = value;
                    InvokePropertyChanged(nameof(Text));
                }
            }
        }
        public ReferenceReadOnlyList<MessageCommand> Commands { get; }

        private readonly EditableCollection<MessageCommand> _commands;
        private readonly bool _isReadOnly;

        #region Статика

        public static Token Create(MessageImportance importance, string title, string text)
        {
            return Create(importance, title, text, null);
        }
        public static Token Create(MessageImportance importance, string title, string text, params IEnumerable<MessageCommand>? commands)
        {
            return new(new(importance, title, text, commands, false));
        }

        #endregion

        #region Классы

        public readonly struct Token(Message message)
        {
            public Message Message { get; } = message;
            public MessageImportance Importance
            {
                get => Message.Importance;
                set => Message.Importance = value;
            }
            public string Title
            {
                get => Message.Title;
                set => Message.Title = value;
            }
            public string Text
            {
                get => Message.Text;
                set => Message.Text = value;
            }
            public EditableCollection<MessageCommand> Commands => Message._commands;
        }

        #endregion
    }
}

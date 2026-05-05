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
            DateTime = DateTime.Now;
        }

        public MessageImportance Importance
        {
            get => field;
            private set
            {
                if (!_isReadOnly && field != value)
                {
                    OnPropertyChanging(nameof(Importance));
                    field = value;
                    OnPropertyChanged(nameof(Importance));
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
                    OnPropertyChanging(nameof(Title));
                    field = value;
                    OnPropertyChanged(nameof(Title));
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
                    OnPropertyChanging(nameof(Text));
                    field = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
        }
        public bool Read
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Read));
                    field = value;
                    OnPropertyChanged(nameof(Read));
                }
            }
        }
        public DateTime DateTime { get; }
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

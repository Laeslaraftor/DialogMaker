using Acly;
using System.ComponentModel;
using DialogMaker.Core.Editor.Messages;
using System.Windows.Controls;
using System.Windows;
using ILogger = DialogMaker.Core.Editor.ILogger;
using System.Windows.Input;
using System.Windows.Media;

namespace DialogMaker.Lib.Elements
{
    public partial class LoggerView : UserControl, ILogger
    {
        public LoggerView()
        {
            InitializeComponent();
            _messages.ItemChanged += OnMessagesItemChanged;

            _messagesList.ItemsSource = _messages;
        }
        ~LoggerView()
        {
            Dispatcher.Invoke(() =>
            {
                var parent = Parent;

                if (parent != null)
                {
                    Mouse.RemovePreviewMouseDownHandler(Parent, OnParentPreviewMouseDown);
                }
            });
        }

        public bool MessagesWindowShowed
        {
            get => (bool)GetValue(MessagesWindowShowedProperty);
            set => SetValue(MessagesWindowShowedProperty, value);
        }

        private readonly EditableCollection<Message> _messages = [];
        private bool _currentMousePressChangedVisibilityState;

        private int UnreadMessages
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;

                    if (value > 0)
                    {
                        _unreadCounter.Visibility = Visibility.Visible;
                        _unreadCounterText.Text = value.ToString();
                        return;
                    }

                    _unreadCounter.Visibility = Visibility.Collapsed;
                }
            }
        }

        #region Управление

        public void Log(object message)
        {
            if (message is Message msg)
            {
                Log(msg);
            }
            else if (message is Exception exception)
            {
                MessageCommand infoCommand = new("Подробнее", p => exception.Alert());
                Log(new(MessageImportance.Critical, exception.GetType().Name, exception.Message, [infoCommand]));
            }
            else if (message != null)
            {
                Log(new(MessageImportance.Normal, string.Empty, message.ToString() ?? string.Empty, null));
            }
        }
        public void Log(Message message)
        {
            _messages.Add(message);
            _messagesScroll.ScrollToEnd();
        }

        private void UpdateUnreadCounter()
        {
            int unreadMessages = 0;

            foreach (var message in _messages)
            {
                if (!message.Read)
                {
                    unreadMessages++;
                }
            }

            UnreadMessages = unreadMessages;
        }
        private void ReadAll()
        {
            foreach (var message in _messages)
            {
                message.Read = true;
            }
        }

        #endregion

        #region События

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            if (oldParent != null)
            {
                Mouse.RemovePreviewMouseDownHandler(oldParent, OnParentPreviewMouseDown);
            }

            var parent = Parent;

            if (parent != null)
            {
                Mouse.AddPreviewMouseDownHandler(Parent, OnParentPreviewMouseDown);
            }
        }

        private async void OnParentPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            bool messagesWindowShowed = MessagesWindowShowed;
            _currentMousePressChangedVisibilityState = false;

            if (!messagesWindowShowed || sender is not UIElement element)
            {
                return;
            }

            var containsWindow = await element.PositionContains(e, obj => obj.Equals(_messagesWindow));

            if (!containsWindow)
            {
                MessagesWindowShowed = false;
                _currentMousePressChangedVisibilityState = true;
            }
        }

        private void OnClearButtonClicked(object sender, RoutedEventArgs e)
        {
            _messages.Clear();
        }
        private void OnShowMessagesWindowButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_currentMousePressChangedVisibilityState)
            {
                return;
            }

            MessagesWindowShowed = true;
        }

        private void OnMessagesItemChanged(object? sender, CollectionItemEventArgs<Message> e)
        {
            if (e.Action == CollectionItemAction.Add)
            {
                e.Item.PropertyChanged += OnMessagePropertyChanged;

                if (string.IsNullOrEmpty(e.Item.Title))
                {
                    _lastMessageText.Text = e.Item.Text;
                }
                else
                {
                    _lastMessageText.Text = $"{e.Item.Title}: {e.Item.Text}";
                }
            }
            else if (e.Action == CollectionItemAction.Remove)
            {
                e.Item.PropertyChanged -= OnMessagePropertyChanged;

                if (_messages.Count == 0)
                {
                    _lastMessageText.Text = string.Empty;
                }
            }
            if (e.Action != CollectionItemAction.Move)
            {
                UpdateUnreadCounter();
            }
        }
        private void OnMessagePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Read")
            {
                UpdateUnreadCounter();
            }
        }

        private static void OnMessagesWindowShowedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LoggerView view && e.NewValue is bool value)
            {
                view._messagesWindow.Visibility = value ? Visibility.Visible : Visibility.Collapsed;

                if (!value)
                {
                    view.ReadAll();
                }
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty MessagesWindowShowedProperty = DependencyProperty.Register(nameof(MessagesWindowShowed), typeof(bool),
            typeof(LoggerView), new(OnMessagesWindowShowedChanged));

        #endregion
    }
}

using DialogMaker.Core.Editor.Messages;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace DialogMaker.Lib.Elements
{
    public partial class MessageView : UserControl
    {
        public MessageView()
        {
            InitializeComponent();
        }

        public Message? Message
        {
            get => GetValue(MessageProperty) as Message;
            set => SetValue(MessageProperty, value);
        }

        #region Управление

        private void SetMessage(Message? oldValue, Message? newValue)
        {
            if (oldValue == newValue)
            {
                return;
            }
            if (oldValue != null)
            {
                oldValue.PropertyChanged -= OnMessagePropertyChanged;
            }

            _commandsList.ItemsSource = newValue?.Commands;

            if (newValue == null)
            {
                return;
            }

            newValue.PropertyChanged += OnMessagePropertyChanged;


            _dateTime.Text = newValue.DateTime.ToString("dd.MM.yyyy H:mm:ss");

            UpdateMessage(newValue);
        }

        private void SetTextOrDisable(TextBlock block, string? text)
        {
            text = text?.Trim();

            if (string.IsNullOrEmpty(text))
            {
                block.Visibility = Visibility.Collapsed;
                return;
            }

            block.Text = text;
            block.Visibility = Visibility.Visible;
        }
        private void UpdateMessage(Message message)
        {
            SetTextOrDisable(_title, message.Title);
            SetTextOrDisable(_text, message.Text);
            _readStatus.Visibility = message.Read ? Visibility.Collapsed : Visibility.Visible;

            if (message.Importance == MessageImportance.Normal)
            {
                _messageImportance.Visibility = Visibility.Collapsed;
                return;
            }
            if (message.Importance == MessageImportance.Warning)
            {
                _messageImportance.Text = "Предупреждения";
            }
            else
            {
                _messageImportance.Text = "Важно";
            }

            _messageImportance.MessageType = (MessageType)message.Importance;
            _messageImportance.Visibility = Visibility.Visible;
        }

        #endregion

        #region События

        private void OnMessagePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is Message message)
            {
                UpdateMessage(message);
            }
        }

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MessageView view)
            {
                view.SetMessage(e.OldValue as Message, e.NewValue as Message);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(Message), typeof(Message),
            typeof(MessageView), new(OnMessageChanged));

        #endregion
    }
}

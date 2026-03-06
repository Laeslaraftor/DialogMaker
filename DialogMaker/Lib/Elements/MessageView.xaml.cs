using Acly.Tokens;
using DialogMaker.Core.Editor.Messages;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace DialogMaker.Lib.Elements
{
    public partial class MessageView : UserControl
    {
        public MessageView()
        {
            InitializeComponent();
        }

        public event EventHandler<EventArgs>? RemoveRequested;

        public Message? Message
        {
            get => GetValue(MessageProperty) as Message;
            set => SetValue(MessageProperty, value);
        }

        private bool _autoRemoveTimerStarted;
        private Token? _currentAutoRemoveToken;

        #region Управление

        public async void StartAutoRemoveTimer(TimeSpan duration)
        {
            if (_autoRemoveTimerStarted)
            {
                return;
            }

            Token currentToken = new();
            _autoRemoveTimerStarted = true;
            _currentAutoRemoveToken = currentToken;

            DoubleAnimation animation = new(RenderSize.Width, 0, duration);
            UpdateAutoRemoveClipRect(RenderSize);

            _autoRemoveProgress.Visibility = Visibility.Visible;
            _autoRemoveProgress.BeginAnimation(WidthProperty, animation);

            await Task.Delay(duration);

            _autoRemoveTimerStarted = false;
            _autoRemoveProgress.Visibility = Visibility.Hidden;

            if (_currentAutoRemoveToken == currentToken)
            {
                RemoveRequested?.Invoke(this, EventArgs.Empty);
            }
        }
        public void CancelAutoRemoveTimer()
        {
            _currentAutoRemoveToken = null;
        }

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
        private void UpdateAutoRemoveClipRect(Size newSize)
        {
            var rect = _autoRemoveProgressClipGeometry.Rect;
            var size = rect.Size;
            size.Width = newSize.Width;

            rect.Size = size;
            _autoRemoveProgressClipGeometry.Rect = rect;
        }

        #endregion

        #region События

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateAutoRemoveClipRect(sizeInfo.NewSize);
        }

        private void OnRemoveButtonClicked(object sender, RoutedEventArgs e)
        {
            RemoveRequested?.Invoke(this, e);
        }

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

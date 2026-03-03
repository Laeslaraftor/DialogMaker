using DialogMaker.Core.Editor.Messages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DialogMaker.Lib.Elements
{
    public partial class ToolTipView : UserControl
    {
        public ToolTipView()
        {
            InitializeComponent();
            SetColorsScheme(ColorsScheme.Normal);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public MessageType MessageType
        {
            get => (MessageType)GetValue(MessageTypeProperty);
            set => SetValue(MessageTypeProperty, value);
        }
        public IEnumerable<MessageCommand>? Commands
        {
            get => GetValue(CommandsProperty) as IEnumerable<MessageCommand>;
            set => SetValue(CommandsProperty, value);
        }

        #region Управление

        private void SetColorsScheme(ColorsScheme scheme)
        {
            _border.Background = scheme.Background;
            _text.Foreground = scheme.Text;
            _commandsList.Foreground = scheme.Text;
        }

        #endregion

        #region События

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToolTipView view)
            {
                view._text.Text = e.NewValue as string;
            }
        }
        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ToolTipView view || 
                e.NewValue is not MessageType type)
            {
                return;
            }

            ColorsScheme scheme = ColorsScheme.Normal;

            switch (type)
            {
                case MessageType.Success:
                    scheme = ColorsScheme.Success;
                    break;
                case MessageType.Warning:
                    scheme = ColorsScheme.Warning;
                    break;
                case MessageType.Error:
                    scheme = ColorsScheme.Error; 
                    break;
            }

            view.SetColorsScheme(scheme);
        }
        private static void OnCommandsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToolTipView view)
            {
                view._commandsList.ItemsSource = e.NewValue as IEnumerable<MessageCommand>;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string),
            typeof(ToolTipView), new(string.Empty, OnTextChanged));
        public static readonly DependencyProperty MessageTypeProperty = DependencyProperty.Register(nameof(MessageType), typeof(MessageType),
            typeof(ToolTipView), new(MessageType.Normal, OnStatusChanged));
        public static readonly DependencyProperty CommandsProperty = DependencyProperty.Register(nameof(Commands), typeof(IEnumerable<MessageCommand>),
            typeof(ToolTipView), new(OnCommandsChanged));

        #endregion

        #region Классы

        private struct ColorsScheme(Brush text, Brush background)
        {
            public Brush Text { get; } = text;
            public Brush Background { get; } = background;

            public static readonly ColorsScheme Normal = new(GetBrush("TextFillColorPrimaryBrush"), GetBrush("SystemFillColorSolidAttentionBackgroundBrush"));
            public static readonly ColorsScheme Success = new(Brushes.Black, GetBrush("SystemFillColorSuccessBrush"));
            public static readonly ColorsScheme Warning = new(Brushes.Black, GetBrush("SystemFillColorCautionBrush"));
            public static readonly ColorsScheme Error = new(Brushes.Black, GetBrush("SystemFillColorCriticalBrush"));

            private static Brush GetBrush(string name)
            {
                return (Brush)Application.Current.Resources[name];
            }
        }

        #endregion
    }
}

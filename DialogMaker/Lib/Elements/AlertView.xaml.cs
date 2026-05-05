using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class AlertView : UserControl
    {
        public AlertView()
        {
            InitializeComponent();
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public object? Message
        {
            get => GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        private readonly TextBlock _textView = new()
        {
            TextWrapping = TextWrapping.Wrap,
        };

        #region События

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AlertView view)
            {
                view._title.Text = e.NewValue.ToString();
            }
        }
        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AlertView view)
            {
                if (e.NewValue is string str)
                {
                    view._content.Content = view._textView;
                    view._textView.Text = str;
                    return;
                }

                view._content.Content = e.NewValue;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string),
            typeof(AlertView), new(string.Empty, OnTitleChanged));
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(Message), typeof(object),
            typeof(AlertView), new(OnMessageChanged));

        #endregion
    }
}

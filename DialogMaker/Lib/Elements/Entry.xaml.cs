using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Lib.Elements
{
    public partial class Entry : UserControl
    {
        public Entry()
        {
            InitializeComponent();
            TextBox = _text;
        }

        public event EventHandler<ValueChangedEventArgs<string>>? ConfirmedText;
        public event EventHandler<TextChangedEventArgs>? TextChanged;

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public ICommand? ConfirmCommand
        {
            get => GetValue(ConfirmCommandProperty) as ICommand;
            set => SetValue(ConfirmCommandProperty, value);
        }
        public TextBox TextBox
        {
            get => (TextBox)GetValue(TextBoxProperty.DependencyProperty);
            private set => SetValue(TextBoxProperty, value);
        }
        public TextWrapping TextWrapping
        {
            get => (TextWrapping)GetValue(TextWrappingProperty);
            set => SetValue(TextWrappingProperty, value);
        }
        public TextAlignment TextAlignment
        {
            get => (TextAlignment)GetValue(TextAlignmentProperty);
            set => SetValue(TextAlignmentProperty, value);
        }
        public bool AcceptsReturn
        {
            get => (bool)GetValue(AcceptsReturnProperty);
            set => SetValue(AcceptsReturnProperty, value);
        }
        public int MaxLines
        {
            get => (int)GetValue(MaxLinesProperty);
            set => SetValue(MaxLinesProperty, value);
        }
        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        private string _startFocusValue = string.Empty;

        #region Управление

        public static implicit operator TextBox(Entry entry)
        {
            return entry.TextBox;
        }

        #endregion

        #region События

        private void OnTextGotFocus(object sender, RoutedEventArgs e)
        {
            _startFocusValue = _text.Text;
        }
        private async void OnTextLostFocus(object sender, RoutedEventArgs e)
        {
            var confirmCommand = ConfirmCommand;
            ValueChangedEventArgs<string> confirmParameter = new(_startFocusValue, _text.Text);

            if (confirmCommand?.CanExecute(confirmParameter) == false)
            {
                return;
            }

            await Task.Delay(50);

            ConfirmedText?.Invoke(this, confirmParameter);
            confirmCommand?.Execute(confirmParameter);
        }
        private void OnTextKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter && !_text.AcceptsReturn) || e.Key == Key.Escape)
            {
                MainWindow.Instance.ClearFocus();
            }
        }
        private void OnTextTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = _text.Text;
            Visibility visibility = Visibility.Visible;

            if (text.Length > 0)
            {
                visibility = Visibility.Collapsed;
            }

            _placeholder.Visibility = visibility;
            Text = text;

            TextChanged?.Invoke(this, e);
        }

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Entry view)
            {
                view._placeholder.Text = e.NewValue as string;  
            }
        }
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Entry view && !view._text.Text.Equals(e.NewValue))
            {
                view._text.Text = e.NewValue as string;
            }
        }
        private static void OnTextWrappingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Entry view)
            {
                view._text.TextWrapping = (TextWrapping)e.NewValue;
            }
        }
        private static void OnTextAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Entry view)
            {
                view._text.TextAlignment = (TextAlignment)e.NewValue;
            }
        }
        private static void OnAcceptsReturnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Entry view)
            {
                view._text.AcceptsReturn = (bool)e.NewValue;
            }
        }
        private static void OnMaxLinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Entry view)
            {
                view._text.MaxLines = (int)e.NewValue;
            }
        }
        private static void OnMaxLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Entry view)
            {
                view._text.MaxLength = (int)e.NewValue;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(nameof(Placeholder), typeof(string),
            typeof(Entry), new(string.Empty, OnPlaceholderChanged));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string),
            typeof(Entry), new(string.Empty, OnTextChanged));
        public static readonly DependencyProperty ConfirmCommandProperty = DependencyProperty.Register(nameof(ConfirmCommand), typeof(ICommand),
           typeof(Entry));
        public static readonly DependencyPropertyKey TextBoxProperty = DependencyProperty.RegisterReadOnly(nameof(TextBox), typeof(TextBox),
           typeof(Entry), new(null));
        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(nameof(TextWrapping), typeof(TextWrapping),
            typeof(Entry), new(TextWrapping.NoWrap, OnTextWrappingChanged));
        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment),
            typeof(Entry), new(TextAlignment.Left, OnTextAlignmentChanged));
        public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty.Register(nameof(AcceptsReturn), typeof(bool),
            typeof(Entry), new(false, OnAcceptsReturnChanged));
        public static readonly DependencyProperty MaxLinesProperty = DependencyProperty.Register(nameof(MaxLines), typeof(int),
            typeof(Entry), new(-1, OnMaxLinesChanged));
        public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register(nameof(MaxLength), typeof(int),
            typeof(Entry), new(-1, OnMaxLengthChanged));

        #endregion
    }
}

using Acly.Tokens;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Lib.Elements
{
    public partial class EditableTextBlock : UserControl
    {
        public EditableTextBlock()
        {
            InitializeComponent();
        }

        public event EventHandler<ValueChangedEventArgs<string>>? TextConfirmed;

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public ICommand? EditCommand
        {
            get => GetValue(EditCommandProperty) as ICommand;
            set => SetValue(EditCommandProperty, value);
        }
        public object? EditCommandParameter
        {
            get => GetValue(EditCommandParameterProperty);
            set => SetValue(EditCommandParameterProperty, value);
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
        public bool AutoEdit
        {
            get => (bool)GetValue(AutoEditProperty);
            set => SetValue(AutoEditProperty, value);
        }

        private bool CanChange => IsEnabled && EditCommand?.CanExecute(EditCommandParameter) != false;
        private bool EditMode
        {
            get => _box.Visibility == Visibility.Visible;
            set
            {
                value &= CanChange;

                if (value == EditMode)
                {
                    return;
                }

                Visibility editorVisibility = Visibility.Hidden;


                if (value)
                {
                    editorVisibility = Visibility.Visible;
                }
                else
                {
                    RemoveLostFocusEvent();
                    InvokeTextConfirmed();
                    Keyboard.ClearFocus();
                }

                _box.Text = Text;
                _block.Visibility = _box.Visibility;
                _box.Visibility = editorVisibility;

                if (value)
                {
                    AddLostFocusEvent();
                }
            }
        }

        private bool _lostFocusEventAdded;
        private Token? _lostFocusEventAddToken;

        #region Управление

        private async void AddLostFocusEvent()
        {
            if (_lostFocusEventAdded)
            {
                return;
            }

            Token token = new();
            _lostFocusEventAddToken = token;

            await Task.Delay(50);

            _box.Focus();

            if (_lostFocusEventAddToken != token)
            {
                return;
            }

            _lostFocusEventAdded = true;
            _box.LostFocus += OnBoxLostFocus;
            _box.PreviewLostKeyboardFocus += OnBoxLostKeyboardFocus;
        }

        private void RemoveLostFocusEvent()
        {
            _lostFocusEventAddToken = null;

            if (_lostFocusEventAdded)
            {
                _box.LostFocus -= OnBoxLostFocus;
                _box.PreviewLostKeyboardFocus -= OnBoxLostKeyboardFocus;
                _lostFocusEventAdded = false;
            }
        }

        private void RemoveFocus()
        {
            RemoveLostFocusEvent();
            EditMode = _box.IsFocused;
        }

        #endregion

        #region События

        private void InvokeTextConfirmed()
        {
            ICommand? editCommand = EditCommand;
            EditCommandEventArgs<string> args = new(Text, _box.Text, EditCommandParameter);

            if (EditCommand != null)
            {
                if (!EditCommand.CanExecute(args))
                {
                    return;
                }

                EditCommand.Execute(args);
            }

            if (AutoEdit)
            {
                Text = args.NewValue;
            }

            TextConfirmed?.Invoke(this, args);
        }

        private void OnBlockMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2 || !CanChange)
            {
                return;
            }

            EditMode = true;
        }
        private void OnBoxLostFocus(object sender, RoutedEventArgs e)
        {
            RemoveFocus();
        }
        private void OnBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            RemoveFocus();
        }
        private void OnBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                e.Handled = true;
                EditMode = false;
            }
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not EditableTextBlock view)
            {
                return;
            }

            string newValue = (string)e.NewValue;

            view._block.Text = newValue;
            view._box.Text = newValue;
        }
        private static void OnTextWrappingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EditableTextBlock view && e.NewValue is TextWrapping value)
            {
                view._block.TextWrapping = value;
                view._box.TextWrapping = value;
            }
        }
        private static void OnTextAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EditableTextBlock view && e.NewValue is TextAlignment value)
            {
                view._block.TextAlignment = value;
                view._box.TextAlignment = value;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string),
            typeof(EditableTextBlock), new(string.Empty, OnTextPropertyChanged));
        public static readonly DependencyProperty EditCommandProperty = DependencyProperty.Register(nameof(EditCommand), typeof(ICommand),
            typeof(EditableTextBlock));
        public static readonly DependencyProperty EditCommandParameterProperty = DependencyProperty.Register(nameof(EditCommandParameter), typeof(object),
            typeof(EditableTextBlock));
        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(nameof(TextWrapping), typeof(TextWrapping),
            typeof(EditableTextBlock), new(TextWrapping.NoWrap, OnTextWrappingChanged));
        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment),
            typeof(EditableTextBlock), new(TextAlignment.Left, OnTextAlignmentChanged));
        public static readonly DependencyProperty AutoEditProperty = DependencyProperty.Register(nameof(AutoEdit), typeof(bool),
            typeof(EditableTextBlock), new(false));

        #endregion
    }
}

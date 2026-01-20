using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Lib.Elements
{
    public partial class CloseView : UserControl
    {
        public CloseView()
        {
            InitializeComponent();
        }

        public event EventHandler<ParameterRoutedEventArgs>? Click;

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public ICommand? CloseCommand
        {
            get => GetValue(CloseCommandProperty) as ICommand;
            set => SetValue(CloseCommandProperty, value);
        }
        public object? CloseCommandParameter
        {
            get => GetValue(CloseCommandParameterProperty);
            set => SetValue(CloseCommandParameterProperty, value);
        }
        public bool CanEditTitle
        {
            get => (bool)GetValue(CanEditTitleProperty);
            set => SetValue(CanEditTitleProperty, value);
        }
        public bool CanClose
        {
            get => (bool)GetValue(CanCloseProperty);
            set => SetValue(CanCloseProperty, value);
        }

        #region События

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            var command = CloseCommand;
            var parameter = CloseCommandParameter;

            if (command?.CanExecute(parameter) == false)
            {
                return;
            }

            Click?.Invoke(this, new(e, parameter));
            command?.Execute(parameter);
        }

        #endregion

        #region События

        private void OnTextTextConfirmed(object sender, ValueChangedEventArgs<string> e)
        {
            Title = e.NewValue;
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CloseView view)
            {
                view._text.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }
        private static void OnCanEditTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CloseView view)
            {
                view._text.IsEnabled = (bool)e.NewValue;
            }
        }
        private static void OnCanCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CloseView view)
            {
                view._closeButtonContainer.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;   
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string),
            typeof(CloseView), new(string.Empty, OnTitleChanged));
        public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand),
            typeof(CloseView));
        public static readonly DependencyProperty CloseCommandParameterProperty = DependencyProperty.Register(nameof(CloseCommandParameter), typeof(object),
            typeof(CloseView));
        public static readonly DependencyProperty CanEditTitleProperty = DependencyProperty.Register(nameof(CanEditTitle), typeof(bool),
            typeof(CloseView), new(true, OnCanEditTitleChanged));
        public static readonly DependencyProperty CanCloseProperty = DependencyProperty.Register(nameof(CanClose), typeof(bool),
            typeof(CloseView), new(true, OnCanCloseChanged));

        #endregion
    }
}

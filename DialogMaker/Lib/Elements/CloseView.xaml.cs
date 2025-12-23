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

        public event EventHandler<RoutedEventArgs>? Click;

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

        #region События

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            var command = CloseCommand;
            var parameter = CloseCommandParameter;

            if (command?.CanExecute(parameter) == false)
            {
                return;
            }

            Click?.Invoke(this, e);
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

        #endregion

        #region Dependency

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string),
            typeof(CloseView), new(string.Empty, OnTitleChanged));
        public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand),
            typeof(CloseView));
        public static readonly DependencyProperty CloseCommandParameterProperty = DependencyProperty.Register(nameof(CloseCommandParameter), typeof(object),
            typeof(CloseView));

        #endregion
    }
}

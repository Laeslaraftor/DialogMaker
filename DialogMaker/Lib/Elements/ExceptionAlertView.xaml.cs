using DialogMaker.Lib.Controllers;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class ExceptionAlertView : UserControl
    {
        public ExceptionAlertView()
        {
            InitializeComponent();
        }

        public Exception? Exception
        {
            get => GetValue(ExceptionProperty) as Exception;
            set => SetValue(ExceptionProperty, value);
        }

        private bool StackTraceIsShown
        {
            get => _stackTraceContainer.Visibility == Visibility.Visible;
            set
            {
                _stackTraceContainer.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                _stackTraceButtonText.Text = value ? "Скрыть" : "Стек вызовов";
            }
        }

        #region Управление

        private void SetException(Exception? oldValue, Exception? newValue)
        {
            _typeName.Text = newValue?.GetType().Name;
            _message.Text = newValue?.Message;
            _stackTrace.Text = newValue?.StackTrace;

            StackTraceIsShown = false;
        }

        #endregion

        #region События

        private void OnShowStackTraceButtonClicked(object sender, RoutedEventArgs e)
        {
            StackTraceIsShown = !StackTraceIsShown;
        }

        private static void OnExceptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ExceptionAlertView view)
            {
                view.SetException(e.OldValue as Exception, e.NewValue as Exception);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty ExceptionProperty = DependencyProperty.Register(nameof(Exception), typeof(Exception),
            typeof(ExceptionAlertView), new(OnExceptionChanged));

        #endregion

        #region Статика

        private static readonly ElementsPool<ExceptionAlertView> _pool = new();

        public static void Show(Exception exception)
        {
            var content = _pool.GetElement();
            content.Exception = exception;

            ModalWindow window = new()
            {
                Child = content,
                Buttons = ModalWindowButtons.Main,
                MainButtonContent = "Продолжить",
                SizeToContent = SizeToContent.Height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            void OnWindowButtonClick(object? sender, ClickValueEventArgs<ModalWindowButtons> e)
            {
                window.Child = null;
                window.Close();
                window.ButtonClick -= OnWindowButtonClick;
            }

            window.ButtonClick += OnWindowButtonClick;

            window.ShowDialog();

            _pool.Free(content);
        }

        #endregion
    }
}

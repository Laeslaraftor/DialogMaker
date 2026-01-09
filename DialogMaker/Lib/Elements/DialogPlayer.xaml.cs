using DialogMaker.Core.Executioning;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace DialogMaker.Lib.Elements
{
    public partial class DialogPlayer : UserControl
    {
        public DialogPlayer()
        {
            InitializeComponent();
            _controlButton.Content = Icons.Play;
        }

        public DialogExecutor? DialogExecutor
        {
            get => GetValue(DialogExecutorProperty) as DialogExecutor;
            set => SetValue(DialogExecutorProperty, value);
        }

        private bool IsRunning
        {
            get => DialogExecutor?.IsRunning == true;
            set
            {
                if (DialogExecutor == null)
                {
                    value = false;
                }

                _controlButton.Content = value ? Icons.Stop : Icons.Play;
            }
        }

        #region Управление

        private void SetDialogExecutor(DialogExecutor? oldValue, DialogExecutor? newValue)
        {
            if (oldValue != null)
            {
                oldValue.DialogHandled -= OnExecutorDialogHandled;
                oldValue.PropertyChanged -= OnDialogExecutorPropertyChanged;

                if (oldValue.IsRunning)
                {
                    oldValue.Stop();
                }
            }
            if (newValue != null)
            {
                newValue.DialogHandled += OnExecutorDialogHandled;
                newValue.PropertyChanged += OnDialogExecutorPropertyChanged;
            }
        }

        #endregion

        #region События

        private void OnExecutorDialogHandled(object? sender, DialogExecutorHandleEventArgs e)
        {
            e.Handle(_viewer);
        }
        private void OnDialogExecutorPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsRunning))
            {
                IsRunning = DialogExecutor?.IsRunning == true;
            }
        }
        private void OnControlButtonClick(object sender, RoutedEventArgs e)
        {
            var executor = DialogExecutor;

            if (executor == null)
            {
                return;
            }
            if (executor.IsRunning)
            {
                executor.Stop();
                return;
            }

            executor.Start();
        }
        private void OnClearButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewer.Clear();
        }

        private static void OnDialogExecutorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DialogPlayer view)
            {
                view._controlButton.IsEnabled = e.NewValue != null;
                view.IsRunning = false;
                view.SetDialogExecutor(e.OldValue as DialogExecutor, e.NewValue as DialogExecutor);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty DialogExecutorProperty = DependencyProperty.Register(nameof(DialogExecutor), typeof(DialogExecutor),
            typeof(DialogPlayer), new(OnDialogExecutorChanged));

        #endregion
    }
}

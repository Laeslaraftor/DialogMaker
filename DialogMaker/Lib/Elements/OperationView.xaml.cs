using DialogMaker.Core.Executioning;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class OperationView : UserControl
    {
        public OperationView()
        {
            InitializeComponent();
            _argumentsList.ItemsSource = _operationArguments;
        }

        public event EventHandler<OperationArgumentClickEventArgs>? ArgumentClicked;

        public Operation Operation
        {
            get => (Operation)GetValue(OperationProperty);
            set => SetValue(OperationProperty, value);
        }

        private readonly ObservableCollection<OperationArgument> _operationArguments = [];

        #region Управление

        private void SetOperation(Operation operation)
        {
            _operationArguments.Clear();
            _operationName.Text = $"{operation.Code} ";

            if (operation.Arguments == null)
            {
                return;
            }

            bool haveManyArguments = operation.Arguments.Length > 1;

            for (int i = 0; i < operation.Arguments.Length; i++)
            {
                _operationArguments.Add(new(operation, i, haveManyArguments && i + 1 < operation.Arguments.Length));
            }
        }

        #endregion

        #region События

        private void OnArgumentItemClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || 
                button.CommandParameter is not OperationArgument argument)
            {
                return;
            }

            ArgumentClicked?.Invoke(this, new(argument.Operation, argument.Index, e));
        }

        private static void OnOperationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OperationView view)
            {
                view.SetOperation((Operation)e.NewValue);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty OperationProperty = DependencyProperty.Register(nameof(Operation), typeof(Operation),
            typeof(OperationView), new(OnOperationChanged));

        #endregion

        #region Классы

        private readonly struct OperationArgument(Operation operation, int index, bool haveSeparator = false)
        {
            public Operation Operation { get; } = operation;
            public int Index { get; } = index;
            public int Value { get; } = operation.Arguments == null ? 0 : operation.Arguments[index];
            public bool HaveSeparator { get; } = haveSeparator;
        }

        #endregion
    }
}

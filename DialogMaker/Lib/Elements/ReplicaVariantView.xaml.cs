using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Lib.Elements
{
    public partial class ReplicaVariantView : UserControl
    {
        public ReplicaVariantView()
        {
            InitializeComponent();
        }

        public event EventHandler<ValueChangedEventArgs<int>>? SelectedIndexChanged;

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public ICommand? SelectIndexCommand
        {
            get => GetValue(SelectIndexCommandProperty) as ICommand;
            set => SetValue(SelectIndexCommandProperty, value);
        }
        public object? SelectIndexCommandParameter
        {
            get => GetValue(SelectIndexCommandParameterProperty);
            set => SetValue(SelectIndexCommandParameterProperty, value);
        }
        public IEnumerable? ItemsSource
        {
            get => GetValue(ItemsSourceProperty) as IEnumerable;
            set => SetValue(ItemsSourceProperty, value);
        }
        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        private bool _isItemsSourceChanging;

        #region Управление

        private void SetItemsSource(IEnumerable? enumerable)
        {
            _isItemsSourceChanging = true;
            _comboBox.ItemsSource = enumerable;
            _comboBox.SelectedIndex = SelectedIndex;
            _isItemsSourceChanging = false;
        }

        #endregion

        #region События

        private void OnComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isItemsSourceChanging)
            {
                return;
            }

            var selectCommand = SelectIndexCommand;
            var selectCommandParameter = SelectIndexCommandParameter;

            if (selectCommand?.CanExecute(selectCommandParameter) == true)
            {
                return;
            }

            selectCommand?.Execute(selectCommandParameter);

            SelectedIndexChanged?.Invoke(this, new(SelectedIndex, _comboBox.SelectedIndex));

            SelectedIndex = _comboBox.SelectedIndex;
        }
        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_textBox.Text != Text)
            {
                Text = _textBox.Text;
            }
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ReplicaVariantView view)
            {
                view._textBox.Text = e.NewValue as string;
            }
        }
        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ReplicaVariantView view)
            {
                view.SetItemsSource(e.NewValue as IEnumerable);
            }
        }
        private static void OnSelectedIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ReplicaVariantView view)
            {
                view._comboBox.SelectedIndex = (int)e.NewValue;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string),
            typeof(ReplicaVariantView), new(string.Empty, OnTextPropertyChanged));
        public static readonly DependencyProperty SelectIndexCommandProperty = DependencyProperty.Register(nameof(SelectIndexCommand), typeof(ICommand),
            typeof(ReplicaVariantView));
        public static readonly DependencyProperty SelectIndexCommandParameterProperty = DependencyProperty.Register(nameof(SelectIndexCommandParameter), typeof(object),
            typeof(ReplicaVariantView));
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable),
            typeof(ReplicaVariantView), new(OnItemsSourcePropertyChanged));
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof(SelectedIndex), typeof(int),
            typeof(ReplicaVariantView), new(OnSelectedIndexPropertyChanged));


        #endregion
    }
}

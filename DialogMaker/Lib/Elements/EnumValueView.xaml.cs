using System.Windows.Controls;
using System.Windows;
using DialogMaker.Core;
using DialogMaker.Core.Editor.Nodes;

namespace DialogMaker.Lib.Elements
{
    public partial class EnumValueView : UserControl
    {
        public EnumValueView()
        {
            InitializeComponent();
        }

        public event EventHandler<RoutedEventArgs>? Click;

        public Enum? Value
        {
            get => GetValue(ValueProperty) as Enum;
            set => SetValue(ValueProperty, value);
        }
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        #region Управление

        private void SetValue(Enum? value)
        {
            if (value == null)
            {
                _enumView.Text = string.Empty;
                return;
            }

            var name = value.GetEnumAttribute<NameAttribute>()?.Name;
            name ??= value.ToString();

            _enumView.Text = name;
        }

        #endregion

        #region События

        private void OnEnumViewClick(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EnumValueView view)
            {
                view.SetValue(e.NewValue as Enum);
            }
        }
        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EnumValueView view)
            {
                view._enumView.IsSelected = (bool)e.NewValue;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(Enum),
            typeof(EnumValueView), new(OnValueChanged));
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSealed), typeof(bool),
            typeof(EnumValueView), new(false, OnIsSelectedChanged));

        #endregion


    }
}

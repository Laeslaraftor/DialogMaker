using DialogMaker.Editor;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class TriggerPresetPortView : UserControl
    {
        public TriggerPresetPortView()
        {
            InitializeComponent();
        }

        public ProjectTriggerPresetPort? Port
        {
            get => GetValue(PortProperty) as ProjectTriggerPresetPort;
            set => SetValue(PortProperty, value);
        }
        public bool OnlyTypeSelector
        {
            get => (bool)GetValue(OnlyTypeSelectorProperty);
            set => SetValue(OnlyTypeSelectorProperty, value);
        }

        #region Управление

        private void SetPort(ProjectTriggerPresetPort? oldValue, ProjectTriggerPresetPort? newValue)
        {
            if (Content is FrameworkElement frameworkElement)
            {
                frameworkElement.DataContext = newValue;
            }

            oldValue?.PropertyChanged -= OnPortPropertyChanged;

            if (newValue != null)
            {
                newValue.PropertyChanged += OnPortPropertyChanged;
                _valueInput.Value = newValue.Value;
            }
        }

        #endregion

        #region События

        private void OnPortPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is ProjectTriggerPresetPort port &&
                e.PropertyName == "Value")
            {
                _valueInput.Value = port.Value;
            }
        }
        private void OnValueInputValueChanged(object sender, ValueChangedEventArgs<object> e)
        {
            Port?.Value = e.NewValue;
        }

        private static void OnPortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TriggerPresetPortView view)
            {
                view.SetPort(e.OldValue as ProjectTriggerPresetPort, e.NewValue as ProjectTriggerPresetPort);
            }
        }
        private static void OnOnlyTypeSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TriggerPresetPortView view && e.NewValue is bool value)
            {
                view._valueInput.OnlyTypeSelector = value;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty PortProperty = DependencyProperty.Register(nameof(Port), typeof(ProjectTriggerPresetPort),
            typeof(TriggerPresetPortView), new(OnPortChanged));
        public static readonly DependencyProperty OnlyTypeSelectorProperty = DependencyProperty.Register(nameof(OnlyTypeSelector), typeof(bool),
            typeof(TriggerPresetPortView), new(false, OnOnlyTypeSelectorChanged));

        #endregion
    }
}

using DialogMaker.Editor;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class TriggerPresetView : UserControl
    {
        public TriggerPresetView()
        {
            InitializeComponent();
        }

        public ProjectTriggerPreset? TriggerPreset
        {
            get => GetValue(TriggerPresetProperty) as ProjectTriggerPreset;
            set => SetValue(TriggerPresetProperty, value);
        }

        #region События

        private static void OnTriggerPresetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TriggerPresetView view && view.Content is FrameworkElement frameworkElement)
            {
                frameworkElement.DataContext = e.NewValue;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty TriggerPresetProperty = DependencyProperty.Register(nameof(TriggerPreset), typeof(ProjectTriggerPreset),
            typeof(TriggerPresetView), new(OnTriggerPresetChanged));

        #endregion
    }
}

using DialogMaker.Core.Executioning.Builders;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class DialogCompilerView : UserControl
    {
        public DialogCompilerView()
        {
            InitializeComponent();
        }

        public DialogCodeBuilder? Builder
        {
            get => GetValue(BuilderProperty) as DialogCodeBuilder;
            set => SetValue(BuilderProperty, value);
        }

        #region Управление

        private void SetBuilder(DialogCodeBuilder? oldValue, DialogCodeBuilder? newValue)
        {
            _sectionView.ItemsSource = newValue?.Sections;
        }

        #endregion

        #region События

        private static void OnBuilderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DialogCompilerView view)
            {
                view.SetBuilder(e.OldValue as DialogCodeBuilder, e.NewValue as DialogCodeBuilder);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty BuilderProperty = DependencyProperty.Register(nameof(Builder), typeof(DialogCodeBuilder),
            typeof(DialogCompilerView), new(OnBuilderChanged));

        #endregion
    }
}

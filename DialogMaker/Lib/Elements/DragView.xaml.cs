using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class DragView : UserControl
    {
        public DragView()
        {
            InitializeComponent();
        }

        public object? Preview
        {
            get => GetValue(PreviewProperty);
            set => SetValue(PreviewProperty, value);
        }

        #region События

        private static void OnPreviewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DragView view)
            {
                view._preview.Content = e.NewValue;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty PreviewProperty = DependencyProperty.Register(nameof(Preview), typeof(object),
            typeof(DragView), new(OnPreviewChanged));

        #endregion
    }
}

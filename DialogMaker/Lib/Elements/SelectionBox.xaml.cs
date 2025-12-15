using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class SelectionBox : UserControl
    {
        public SelectionBox()
        {
            InitializeComponent();
        }

        public Point StartPoint
        {
            get => (Point)GetValue(StartPointProperty);
            set => SetValue(StartPointProperty, value);
        }
        public Point EndPoint
        {
            get => (Point)GetValue(EndPointProperty);
            set => SetValue(EndPointProperty, value);
        }

        #region Управление

        private void UpdateSize()
        {
            var startPoint = StartPoint;
            var endPoint = EndPoint;
            var translationPoint = startPoint;
            var size = startPoint - endPoint;

            if (startPoint.X > endPoint.X)
            {
                translationPoint.X = endPoint.X;
            }
            if (startPoint.Y > endPoint.Y)
            {
                translationPoint.Y = endPoint.Y;
            }

            _translation.X = translationPoint.X;
            _translation.Y = translationPoint.Y;

            Width = Math.Abs(size.X);
            Height = Math.Abs(size.Y);
        }

        #endregion

        #region События

        private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SelectionBox view)
            {
                view.UpdateSize();
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register(nameof(StartPoint), typeof(Point),
            typeof(SelectionBox), new(OnSizeChanged));
        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register(nameof(EndPoint), typeof(Point),
            typeof(SelectionBox), new(OnSizeChanged));

        #endregion
    }
}

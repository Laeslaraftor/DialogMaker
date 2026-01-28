using Acly;
using DialogMaker.Lib.Controllers;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DialogMaker.Lib.Elements
{
    public partial class FaceView : UserControl
    {
        public FaceView()
        {
            InitializeComponent();
        }

        public double LeftEyebrowVerticalPosition
        {
            get => (double)GetValue(LeftEyebrowVerticalPositionProperty);
            set => SetValue(LeftEyebrowVerticalPositionProperty, value);
        }
        public double LeftEyebrowRotation
        {
            get => (double)GetValue(LeftEyebrowRotationProperty);
            set => SetValue(LeftEyebrowRotationProperty, value);
        }
        public double LeftEyeClosedValue
        {
            get => (double)GetValue(LeftEyeClosedValueProperty);
            set => SetValue(LeftEyeClosedValueProperty, value);
        }
        public double RightEyebrowVerticalPosition
        {
            get => (double)GetValue(RightEyebrowVerticalPositionProperty);
            set => SetValue(RightEyebrowVerticalPositionProperty, value);
        }
        public double RightEyebrowRotation
        {
            get => (double)GetValue(RightEyebrowRotationProperty);
            set => SetValue(RightEyebrowRotationProperty, value);
        }
        public double RightEyeClosedValue
        {
            get => (double)GetValue(RightEyeClosedValueProperty);
            set => SetValue(RightEyeClosedValueProperty, value);
        }
        public double MouthOpenedValue
        {
            get => (double)GetValue(MouthOpenedValueProperty);
            set => SetValue(MouthOpenedValueProperty, value);
        }
        public double MouthHorizontalStretch
        {
            get => (double)GetValue(MouthHorizontalStretchProperty);
            set => SetValue(MouthHorizontalStretchProperty, value);
        }
        public double LeftMouthCornerVerticalPosition
        {
            get => (double)GetValue(LeftMouthCornerVerticalPositionProperty);
            set => SetValue(LeftMouthCornerVerticalPositionProperty, value);
        }
        public double RightMouthCornerVerticalPosition
        {
            get => (double)GetValue(RightMouthCornerVerticalPositionProperty);
            set => SetValue(RightMouthCornerVerticalPositionProperty, value);
        }

        #region Управление

        private void SetLeftEyeClosedValue(double value)
        {
            SetEyeClosedValue(_leftEyeTop, value);
        }
        private void SetRightEyeClosedValue(double value)
        {
            SetEyeClosedValue(_rightEyeTop, value);
        }
        private void SetMouthOpenValue(double value)
        {
            var top = _mouthTopLip.Points[1];
            var centerTop = _mouthCenterTop.Points[1];
            var bottom = _mouthBottomLip.Points[1];
            var centerBottom = _mouthCenterBottom.Points[1];

            double lipsValue = Helper.LerpUnclamped(80, 150, value);

            top.Y = -lipsValue;
            centerTop.Y = Helper.LerpUnclamped(-5, -85, value);
            bottom.Y = lipsValue;
            centerBottom.Y = Helper.LerpUnclamped(-5, 75, value);

            _mouthTopLip.Points[1] = top;
            _mouthCenterTop.Points[1] = centerTop;
            _mouthBottomLip.Points[1] = bottom;
            _mouthCenterBottom.Points[1] = centerBottom;
        }
        private void SetMouthLeftCornerValue(double value)
        {
            SetVerticalCornersValue(value, 2, _mouthTopLip, _mouthCenterBottom, _mouthCenterTop);
            SetVerticalCornersValue(value, 0, _mouthBottomLip);
        }
        private void SetMouthRightCornerValue(double value)
        {
            void SetY(double value, params PathFigure[] figures)
            {
                foreach (var figure in figures)
                {
                    var startPoint = figure.StartPoint;
                    startPoint.Y = value;
                    figure.StartPoint = startPoint;
                }
            }

            var y = SetVerticalCornersValue(value, 0, _mouthTopLip, _mouthCenterBottom, _mouthCenterTop);
            SetVerticalCornersValue(value, 2, _mouthBottomLip);

            SetY(y, _mouthLipsFigure, _mouthCenterTopFigure, _mouthCenterBottomFigure);
        }
        private void SetMouthHorizontalValue(double value)
        {
            _mouthScale.ScaleX = Helper.LerpUnclamped(1, 0.4, value);
        }

        private static double SetVerticalCornersValue(double value, int pointIndex, params PolyBezierSegment[] segments)
        {
            value = Helper.LerpUnclamped(30, -30, (value + 1) / 2);

            SetCornersValue(point =>
            {
                point.Y = value;
                return point;
            }, pointIndex, segments);

            return value;
        }
        private static void SetCornersValue(Func<Point, Point> handler, int pointIndex, params PolyBezierSegment[] segments)
        {
            foreach (var segment in segments)
            {
                var point = handler(segment.Points[pointIndex]);
                segment.Points[pointIndex] = point;
            }
        }

        private static void SetEyeClosedValue(PolyBezierSegment segment, double value)
        {
            var first = segment.Points[0];
            var second = segment.Points[1];

            first.Y = Helper.LerpUnclamped(0, 80, value);
            second.Y = Helper.LerpUnclamped(0, 120, value);

            segment.Points[0] = first;
            segment.Points[1] = second;
        }
        private void SetEyebrowVerticalPosition(Path eyebrow, double value)
        {
            Canvas.SetTop(eyebrow, Helper.LerpUnclamped(30, -70, value));
        }
        private void SetEyebrowRotation(RotateTransform rotation, double value)
        {
            value = (value + 1) / 2;
            rotation.Angle = Helper.LerpUnclamped(-30, 30, value);
        }

        #endregion

        #region События

        private static void OnLeftEyebrowVerticalPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FaceView view && e.NewValue is double value)
            {
                view.SetEyebrowVerticalPosition(view._leftEyebrow, value);
            }
        }
        private static void OnLeftEyebrowRotationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FaceView view && e.NewValue is double value)
            {
                view.SetEyebrowRotation(view._leftEyebrowRotation, value);
            }
        }
        private static void OnLeftEyeClosedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FaceView view)
            {
                view.SetLeftEyeClosedValue((double)e.NewValue);
            }
        }
        private static void OnRightEyebrowVerticalPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FaceView view && e.NewValue is double value)
            {
                view.SetEyebrowVerticalPosition(view._rightEyebrow, value);
            }
        }
        private static void OnRightEyebrowRotationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FaceView view && e.NewValue is double value)
            {
                view.SetEyebrowRotation(view._rightEyebrowRotation, value);
            }
        }
        private static void OnRightEyeClosedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FaceView view)
            {
                view.SetRightEyeClosedValue((double)e.NewValue);
            }
        }
        private static void OnMouthOpenedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FaceView view)
            {
                view.SetMouthOpenValue((double)e.NewValue);
            }
        }
        private static void OnMouthHorizontalStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FaceView view)
            {
                view.SetMouthHorizontalValue((double)e.NewValue);
            }
        }
        private static void OnLeftMouthCornerVerticalPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FaceView view)
            {
                view.SetMouthLeftCornerValue((double)e.NewValue);
            }
        }
        private static void OnRightMouthCornerVerticalPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FaceView view)
            {
                view.SetMouthRightCornerValue((double)e.NewValue);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty LeftEyebrowVerticalPositionProperty =
            DependencyProperty.Register(nameof(LeftEyebrowVerticalPosition), typeof(double),
                typeof(FaceView), new(OnLeftEyebrowVerticalPositionChanged));
        public static readonly DependencyProperty LeftEyebrowRotationProperty =
            DependencyProperty.Register(nameof(LeftEyebrowRotation), typeof(double),
                typeof(FaceView), new(OnLeftEyebrowRotationChanged));
        public static readonly DependencyProperty LeftEyeClosedValueProperty =
            DependencyProperty.Register(nameof(LeftEyeClosedValue), typeof(double),
                typeof(FaceView), new(OnLeftEyeClosedValueChanged));
        public static readonly DependencyProperty RightEyebrowVerticalPositionProperty =
            DependencyProperty.Register(nameof(RightEyebrowVerticalPosition), typeof(double),
                typeof(FaceView), new(OnRightEyebrowVerticalPositionChanged));
        public static readonly DependencyProperty RightEyebrowRotationProperty =
            DependencyProperty.Register(nameof(RightEyebrowRotation), typeof(double),
                typeof(FaceView), new(OnRightEyebrowRotationChanged));
        public static readonly DependencyProperty RightEyeClosedValueProperty =
            DependencyProperty.Register(nameof(RightEyeClosedValue), typeof(double),
                typeof(FaceView), new(OnRightEyeClosedValueChanged));
        public static readonly DependencyProperty MouthOpenedValueProperty =
            DependencyProperty.Register(nameof(MouthOpenedValue), typeof(double),
                typeof(FaceView), new(OnMouthOpenedValueChanged));
        public static readonly DependencyProperty MouthHorizontalStretchProperty =
            DependencyProperty.Register(nameof(MouthHorizontalStretch), typeof(double),
                typeof(FaceView), new(OnMouthHorizontalStretchChanged));
        public static readonly DependencyProperty LeftMouthCornerVerticalPositionProperty =
            DependencyProperty.Register(nameof(LeftMouthCornerVerticalPosition), typeof(double),
                typeof(FaceView), new(OnLeftMouthCornerVerticalPositionChanged));
        public static readonly DependencyProperty RightMouthCornerVerticalPositionProperty =
            DependencyProperty.Register(nameof(RightMouthCornerVerticalPosition), typeof(double),
                typeof(FaceView), new(OnRightMouthCornerVerticalPositionChanged));

        #endregion
    }
}

using Acly;
using Acly.Numbers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DialogMaker.Lib.Elements
{
    public class CurveLine : Shape, IDisposable
    {
        public CurveLine()
        {
            _figure = new();
            _path = new()
            {
                Data = new PathGeometry([_figure])
            };

            StrokeDashArray = [];

            Canvas canvas = new();
            canvas.Children.Add(_path);
        }
        ~CurveLine()
        {
            Dispose();
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
        public double Offset
        {
            get => (double)GetValue(OffsetProperty);
            set => SetValue(OffsetProperty, value);
        }
        public int Resolution
        {
            get => (int)GetValue(ResolutionProperty);
            set => SetValue(ResolutionProperty, value);
        }
        public Easing Easing
        {
            get => (Easing)GetValue(EasingProperty);
            set => SetValue(EasingProperty, value);
        }

        protected override Geometry DefiningGeometry => _path.Data;

        private readonly Path _path;
        private readonly PathFigure _figure;
        private readonly ElementsPool<LineSegment> _segmentsPool = new();

        #region Управление

        public void Dispose()
        {
            _segmentsPool.Dispose();
            GC.SuppressFinalize(this);
        }

        private void UpdateCurve()
        {
            _figure.Segments.Clear();
            _segmentsPool.Clear();

            Func<float, float> easing = EasingFunctions.GetEasingFunction(Easing);
            Point from = StartPoint;
            Point to = EndPoint;
            Point endPoint = to;
            double offset = Offset;
            int resolution = Resolution;

            if (from == to)
            {
                return;
            }

            void AddSegment(Point position)
            {
                var segment = _segmentsPool.GetElement();
                segment.Point = position;

                _figure.Segments.Add(segment);
            }

            if (from.X > to.X)
            {
                (to, from) = (from, to);
            }

            _figure.StartPoint = from;

            if (offset != 0)
            {
                endPoint = new(to.X, to.Y);
                from.X += offset;
                to.X -= offset;
            }

            for (int i = 0; i < resolution; i++)
            {
                float percent = (float)i / resolution;

                Point position = new()
                {
                    X = Helper.LerpUnclamped(from.X, to.X, percent),
                    Y = Helper.LerpUnclamped(from.Y, to.Y, easing(percent)),
                };

                AddSegment(position);
            }

            AddSegment(endPoint);
        }

        #endregion

        #region События

        private static void OnCurveParamChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CurveLine view)
            {
                view.UpdateCurve();
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register(nameof(StartPoint), typeof(Point),
            typeof(CurveLine), new(OnCurveParamChanged));
        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register(nameof(EndPoint), typeof(Point),
            typeof(CurveLine), new(OnCurveParamChanged));
        public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(nameof(Offset), typeof(double),
            typeof(CurveLine), new(OnCurveParamChanged));
        public static readonly DependencyProperty EasingProperty = DependencyProperty.Register(nameof(Easing), typeof(Easing),
            typeof(CurveLine), new(Easing.EaseInOutCubic, OnCurveParamChanged));
        public static readonly DependencyProperty ResolutionProperty = DependencyProperty.Register(nameof(Resolution), typeof(int),
            typeof(CurveLine), new(8, OnCurveParamChanged));

        #endregion
    }
}

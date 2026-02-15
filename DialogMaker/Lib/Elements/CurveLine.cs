using Acly;
using Acly.Numbers;
using System.Diagnostics;
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
        public bool Inverse
        {
            get => (bool)GetValue(InverseProperty);
            set => SetValue(InverseProperty, value);
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

            Point from = StartPoint;
            Point to = EndPoint;
            double offset = Offset;
            int resolution = Resolution;
            bool invertPoints = Inverse;

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
                invertPoints = !invertPoints;
            }
            if (offset != 0)
            {
                if (invertPoints)
                {
                    from.X += offset;
                    to.X -= offset;
                }
                else
                {
                    from.X -= offset;
                    to.X += offset;
                }
            }

            _figure.StartPoint = from;

            foreach (var line in GetSmoothPoints(from, to, invertPoints))
            {
                for (int i = 0; i < resolution; i++)
                {
                    float percent = (float)i / resolution;
                    var position = line.Lerp(percent);

                    AddSegment(position);
                }
            }
        }
        private IEnumerable<Line> GetSmoothPoints(Point start, Point end, bool invert)
        {
            Func<double, double, bool, double> addOrSubtractX;

            if (invert)
            {
                addOrSubtractX = (v1, v2, localInvert) =>
                {
                    if (localInvert)
                    {
                        return v1 + v2;
                    }

                    return v1 - v2;
                };
            }
            else
            {
                addOrSubtractX = (v1, v2, localInvert) =>
                {
                    if (localInvert)
                    {
                        return v1 - v2;
                    }

                    return v1 + v2;
                };
            }

            double offset = 25;
            double horizontalDistance = Math.Abs(end.X - start.X);
            double verticalDistance = Math.Abs(end.Y - start.Y);

            double percentOfTransition;
            double horizontalSmoothDistance;

            if (invert)
            {
                percentOfTransition = offset / 80;
                horizontalSmoothDistance = offset / 1.5;
            }
            else
            {
                percentOfTransition = Helper.Clamp01((horizontalDistance + offset / 2) / 80);
                horizontalSmoothDistance = Helper.Lerp(offset, horizontalDistance / 2, percentOfTransition);
            }

            double verticalSmoothDistance = Math.Min(verticalDistance / 6, offset);
            verticalSmoothDistance = Helper.Lerp(verticalSmoothDistance, verticalDistance / 2, percentOfTransition);

            Point firstEnd = new()
            {
                X = addOrSubtractX(start.X, horizontalSmoothDistance, false),
                Y = end.Y > start.Y ? start.Y + verticalSmoothDistance : start.Y - verticalSmoothDistance
            };

            yield return new(start, firstEnd, EasingFunctions.Cubic.In);

            Point secondEnd;

            if (percentOfTransition < 1)
            {
                secondEnd = new()
                {
                    X = addOrSubtractX(end.X, horizontalSmoothDistance, true),
                    Y = end.Y > start.Y ? end.Y - verticalSmoothDistance : end.Y + verticalSmoothDistance
                };

                yield return new(firstEnd, secondEnd, EasingFunctions.Cubic.InOut)
                {
                    LerpFunc = Line.XLerp
                };
            }
            else
            {
                secondEnd = firstEnd;
            }

            yield return new(secondEnd, end, EasingFunctions.Cubic.Out);
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
        public static readonly DependencyProperty InverseProperty = DependencyProperty.Register(nameof(Inverse), typeof(bool),
            typeof(CurveLine), new(false, OnCurveParamChanged));
        public static readonly DependencyProperty ResolutionProperty = DependencyProperty.Register(nameof(Resolution), typeof(int),
            typeof(CurveLine), new(8, OnCurveParamChanged));

        #endregion

        #region Классы

        private struct Line()
        {
            public Line(Point start, Point end, Easing easing)
                : this(start, end, EasingFunctions.GetEasingFunction(easing))
            {
            }
            public Line(Point start, Point end, Func<float, float> easing)
                : this(start, end)
            {
                Easing = easing;
            }
            public Line(Point start, Point end)
                : this()
            {
                Start = start;
                End = end;
            }

            public Point Start { get; set; }
            public Point End { get; set; }
            public Func<float, float> Easing { get; set; } = EasingFunctions.Linear;
            public Func<Line, float, Point> LerpFunc { get; set; } = YLerp;

            public Point Lerp(float percent)
            {
                if (LerpFunc == null)
                {
                    return Start;
                }

                return LerpFunc(this, percent);
            }

            public static Point XLerp(Line line, float percent)
            {
                return new()
                {
                    X = Helper.LerpUnclamped(line.Start.X, line.End.X, line.Easing(percent)),
                    Y = Helper.LerpUnclamped(line.Start.Y, line.End.Y, percent),
                };
            }
            public static Point YLerp(Line line, float percent)
            {
                return new()
                {
                    X = Helper.LerpUnclamped(line.Start.X, line.End.X, percent),
                    Y = Helper.LerpUnclamped(line.Start.Y, line.End.Y, line.Easing(percent)),
                };
            }
        }

        #endregion
    }
}

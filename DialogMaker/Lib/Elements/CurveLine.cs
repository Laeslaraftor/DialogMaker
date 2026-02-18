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
        public bool DirectStart
        {
            get => (bool)GetValue(DirectStartProperty);
            set => SetValue(DirectStartProperty, value);
        }
        public bool DirectEnd
        {
            get => (bool)GetValue(DirectEndProperty);
            set => SetValue(DirectEndProperty, value);
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
            bool directStart = DirectStart;
            bool directEnd = DirectEnd;

            if (from == to)
            {
                return;
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

            Point lastSegment = from;

            void AddSegment(Point position)
            {
                if (lastSegment == position)
                {
                    return;
                }

                lastSegment = position;
                var segment = _segmentsPool.GetElement();
                segment.Point = position;

                _figure.Segments.Add(segment);
            }

            _figure.StartPoint = from;

            if (!directStart && !directEnd && invertPoints)
            {
                resolution *= 2;
            }

            var points = GetCurvePoints(from, to, invertPoints, directStart, directEnd);
            var pointsArray = points.ToArray();

            for (int i = 0; i < resolution; i++)
            {
                float percent = (float)i / resolution;
                var position = Line.Lerp(pointsArray, percent, EasingFunctions.Cubic.InOut);

                AddSegment(position);
            }

            AddSegment(to);
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
        private IEnumerable<Line> GetCurvePoints(Point start, Point end, bool invert, bool directStart, bool directEnd)
        {
            if (invert)
            {
                (directStart, directEnd) = (directEnd, directStart);
            }
            if (directStart && directEnd)
            {
                invert = !invert;
                directStart = false;
                directEnd = false;
            }

            const double maxOffset = 25;
            double scalePercent = 0;

            Point offset = new()
            {
                X = maxOffset,
                Y = Math.Min(Math.Abs(start.Y - end.Y) / 8, maxOffset)
            };
            Point delta = new()
            {
                X = end.X - start.X,
                Y = end.Y - start.Y
            };

            scalePercent = Helper.Clamp01(-delta.X / maxOffset);

            if (!invert)
            {
                scalePercent = 1 - scalePercent;
            }

            Point halfDelta = new()
            {
                X = Math.Abs(delta.X / 2),
                Y = Math.Abs(delta.Y / 2),
            };

            offset += halfDelta * scalePercent;
            offset.X = Math.Min(offset.X, halfDelta.X);

            if (directStart || directEnd)
            {
                offset.X = Math.Min(offset.X, maxOffset);
            }

            offset.Y = Math.Min(offset.Y, halfDelta.Y);

            Point startOffset = new()
            {
                X = AddOrSubtract(0, offset.X, (false ^ invert) ^ (!invert && directStart)),
                Y = AddOrSubtract(0, offset.Y, start.Y > end.Y)
            };
            Point endOffset = new()
            {
                X = AddOrSubtract(0, offset.X, !invert ^ (!invert && directEnd)),
                Y = AddOrSubtract(0, offset.Y, start.Y < end.Y)
            };

            Point first = start + startOffset;
            Point second = end + endOffset;

            if (!directStart && !directEnd)
            {
                yield return new(start, first, EasingFunctions.Cubic.In);
                yield return new(first, second, EasingFunctions.Cubic.InOut)
                {
                    LerpFunc = Line.XLerp
                };
                yield return new(second, end, EasingFunctions.Cubic.Out);
            }
            else if ((invert && directStart) || (!invert && directEnd))
            {
                yield return new(start, second, EasingFunctions.Cubic.Out)
                {
                    LerpFunc = Line.XLerp
                };
                yield return new(second, end, EasingFunctions.Cubic.Out);
            }
            else if ((invert && directEnd) || (!invert && directStart))
            {
                yield return new(start, first, EasingFunctions.Cubic.In);
                yield return new(first, end, EasingFunctions.Cubic.In)
                {
                    LerpFunc = Line.XLerp
                };
            }
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
        public static readonly DependencyProperty DirectStartProperty = DependencyProperty.Register(nameof(DirectStart), typeof(bool),
            typeof(CurveLine), new(false, OnCurveParamChanged));
        public static readonly DependencyProperty DirectEndProperty = DependencyProperty.Register(nameof(DirectEnd), typeof(bool),
            typeof(CurveLine), new(false, OnCurveParamChanged));

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
            public static Point Lerp(Line[] lines, double time, Func<float, float> easing)
            {
                if (lines.Length == 0)
                {
                    return new();
                }

                double lineSize = 1d / lines.Length;
                int partIndex = (int)Math.Floor(time / lineSize);

                if (0 > partIndex)
                {
                    return lines[0].Start;
                }
                else if (partIndex >= lines.Length)
                {
                    return lines[^1].End;
                }

                time = (time - (lineSize * partIndex)) / lineSize;
                var easingValue = easing((float)time);
                var line = lines[partIndex];

                return line.Lerp((float)time);
                return new()
                {
                    X = Helper.LerpUnclamped(line.Start.X, line.End.X, time),
                    Y = Helper.LerpUnclamped(line.Start.Y, line.End.Y, easingValue)
                };
            }
        }

        #endregion

        #region Статика

        private static double AddOrSubtract(double v1, double v2, bool localInvert)
        {
            if (localInvert)
            {
                return v1 - v2;
            }

            return v1 + v2;
        }

        #endregion
    }
}

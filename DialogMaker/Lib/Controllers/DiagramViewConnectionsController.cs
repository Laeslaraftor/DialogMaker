using Acly.Numbers;
using DialogMaker.Editor;
using DialogMaker.Lib.Elements;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DialogMaker.Lib.Controllers
{
    public class DiagramViewConnectionsController : IDisposable
    {
        public DiagramViewConnectionsController(DiagramView view, Canvas canvas)
        {
            View = view;
            Canvas = canvas;

            view.PortPressed += OnNodePortPressed;
            view.PreviewMouseMove += OnViewPreviewMouseMove;
            view.PreviewMouseUp += OnViewPreviewMouseUp; ;
        }
        ~DiagramViewConnectionsController()
        {
            Dispose();
        }

        public DiagramView View { get; }
        public Canvas Canvas { get; }
        public double CurvesThickness
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;

                    foreach (var curve in _curves)
                    {
                        curve.Line.StrokeThickness = value;
                    }
                }
            }
        }
        public double CurvesOffset
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;

                    foreach (var curve in _curves)
                    {
                        curve.Line.Offset = value;
                    }
                }
            }
        }
        public Easing CurvesEasing
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;

                    foreach (var curve in _curves)
                    {
                        curve.Line.Easing = value;
                    }
                }
            }
        }
        public int CurvesResolution
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;

                    foreach (var curve in _curves)
                    {
                        curve.Line.Resolution = value;
                    }
                }
            }
        }
        public ProjectDialog? Dialog
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    UpdateConnections();
                }
            }
        }

        private readonly ElementsPool<CurveLine> _curvesPool = new();
        private readonly List<Curve> _curves = [];
        private Curve? _currentCurve;

        #region Управление

        public void UpdateConnections()
        {
            Clear();

            if (Dialog == null)
            {
                return;
            }

            var connections = Dialog.GetConnections();

            foreach (var info in connections)
            {
                foreach (var end in info.Value)
                {
                    var curve = GetCurve(info.Key, true);
                    curve.EndPort = end;
                }
            }
        }
        public void UpdateConnections(DialogProjectNode node)
        {
            void CheckPorts(IEnumerable<DialogProjectNodePortProxy> ports)
            {
                foreach (var port in ports)
                {
                    CheckPort(port);
                }
            }
            void CheckPort(DialogProjectNodePortProxy port)
            {
                foreach (var curve in _curves)
                {
                    if (curve.StartPort == port ||
                        curve.EndPort == port)
                    {
                        curve.SyncPositions();
                    }
                }
            }

            CheckPorts(node.Inputs);
            CheckPorts(node.Outputs);
        }

        public void Dispose()
        {
            View.PortPressed -= OnNodePortPressed;
            View.PreviewMouseMove -= OnViewPreviewMouseMove;
            View.PreviewMouseUp -= OnViewPreviewMouseUp;

            GC.SuppressFinalize(this);
        }

        private void Clear()
        {
            foreach (var curve in _curves)
            {
                RemoveCurve(curve, false);
            }

            _curves.Clear();
        }

        private void RemoveCurve(Curve curve, bool fullRemove = true)
        {
            if (fullRemove)
            {
                _curves.Remove(curve);
            }

            Canvas.Children.Remove(curve.Line);
            _curvesPool.Free(curve.Line);
        }
        private Curve GetCurve(DialogProjectNodePortProxy port, bool unique = false)
        {
            if (TryGetPort(port, out var result) && !unique)
            {
                return result;
            }

            var view = _curvesPool.GetElement();
            view.StrokeThickness = CurvesThickness;
            view.Resolution = CurvesResolution;
            view.Offset = CurvesOffset;
            view.Easing = CurvesEasing;
            result = new(view, Canvas)
            {
                StartPort = port
            };


            Canvas.Children.Add(view);
            _curves.Add(result);

            return result;
        }
        private bool TryGetPort(DialogProjectNodePortProxy port, [NotNullWhen(true)] out Curve? curve)
        {
            curve = null;

            foreach (var info in _curves)
            {
                if (info.StartPort == port ||
                    info.EndPort == port)
                {
                    curve = info;
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region События

        private void OnNodePortPressed(object? sender, ItemMouseEventArgs<DialogProjectNodePortProxy> e)
        {
            if (e.Mouse.LeftButton != MouseButtonState.Pressed ||
                _currentCurve != null)
            {
                return;
            }

            e.Mouse.Handled = true;
            _currentCurve = GetCurve(e.Item);
            _currentCurve.SyncPositions();
        }
        private async void OnViewPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Released ||
                _currentCurve == null)
            {
                return;
            }

            var position = e.GetPosition(Canvas);
            DialogProjectNodePortProxy? endPort = null;
            bool curveFound = false;

            await Canvas.Fetch(position, o =>
            {
                if (o is DiagramNodePort port && 
                    port.DataContext is DialogProjectNodePortProxy proxy)
                {
                    endPort = proxy;
                }
            }, result =>
            {
                if (curveFound || endPort != null)
                {
                    return true;
                }

                curveFound = result.VisualHit is CurveLine;
                return false;
            });

            if (!_currentCurve.SetConnection(endPort) || endPort == null)
            {
                RemoveCurve(_currentCurve);
            }
            else
            {
                _currentCurve.SyncPositions();
            }

            _currentCurve = null;
        }
        private void OnViewPreviewMouseMove(object sender, MouseEventArgs e)
        {
            _currentCurve?.EndPosition = e.GetPosition(Canvas);
        }

        #endregion

        #region Классы

        private class Curve
        {
            public Curve(CurveLine line, Visual container)
            {
                Line = line;
                Container = container;
                _gradient = new(line)
                {
                    StartColor = Colors.White,
                    EndColor = Colors.White,
                };
            }

            public CurveLine Line { get; }
            public Visual Container { get; }
            public DialogProjectNodePortProxy? StartPort
            {
                get => field;
                set
                {
                    if (field == value)
                    {
                        return;
                    }

                    field = value;

                    if (value == null)
                    {
                        Line.Visibility = Visibility.Collapsed;
                        return;
                    }

                    Line.Visibility = Visibility.Visible;

                    _gradient.StartColor = value.Color.Color;
                    StartPosition = value.View.GetConnectorPosition(Container);
                }
            }
            public DialogProjectNodePortProxy? EndPort
            {
                get => field;
                set
                {
                    if (field == value)
                    {
                        return;
                    }

                    field = value;

                    if (value == null)
                    {
                        _gradient.EndColor = Colors.White;
                        return;
                    }

                    _gradient.EndColor = value.Color.Color;
                    EndPosition = value.View.GetConnectorPosition(Container);
                }
            }
            public Point StartPosition
            {
                get => Line.StartPoint;
                set => Line.StartPoint = value;
            }
            public Point EndPosition
            {
                get => Line.EndPoint;
                set
                {
                    Line.EndPoint = value;
                    var gradient = _gradient;
                    gradient.Invert = StartPosition.X > EndPosition.X;
                }
            }

            private readonly Gradient _gradient;

            public void SyncPositions()
            {
                if (StartPort != null)
                {
                    StartPosition = StartPort.View.GetConnectorPosition(Container);
                }
                if (EndPort != null)
                {
                    EndPosition = EndPort.View.GetConnectorPosition(Container);
                }
            }
            public bool SetConnection(DialogProjectNodePortProxy? port)
            {
                if (StartPort == null)
                {
                    return false;
                }
                if (port == null && EndPort != null)
                {
                    StartPort.Original.Disconnect(EndPort.Original);
                }
                else if (port != null && port != EndPort)
                {
                    if (EndPort != null && 
                        port != EndPort &&
                        StartPort.Original.IsConnected(EndPort.Original))
                    {
                        StartPort.Original.Disconnect(EndPort.Original);
                    }

                    try
                    {
                        StartPort.Original.Connect(port.Original);
                    }
                    catch (Exception error)
                    {
                        error.Alert();
                        return false;
                    }
                }

                EndPort = port;

                return true;
            }
        }
        private class Gradient
        {
            public Gradient(CurveLine line)
            {
                Line = line;

                if (line.Stroke is not LinearGradientBrush gradient)
                {
                    gradient = new();
                    line.Stroke = gradient;
                }

                gradient.StartPoint = new();
                gradient.EndPoint = new(1, 0);

                if (gradient.GradientStops.Count < 2)
                {
                    for (int i = gradient.GradientStops.Count; i < 2; i++)
                    {
                        gradient.GradientStops.Add(new()
                        {
                            Offset = i
                        });
                    }
                }

                _start = gradient.GradientStops[0];
                Start = _start;
                _end = gradient.GradientStops[1];
                End = _end;

                for (int i = 0; i < gradient.GradientStops.Count - 2; i++)
                {
                    gradient.GradientStops.RemoveAt(gradient.GradientStops.Count - 1);
                }
            }

            public CurveLine Line { get; }
            public Color StartColor
            {
                get => Start.Color;
                set => Start.Color = value;
            }
            public Color EndColor
            {
                get => End.Color;
                set => End.Color = value;
            }
            public bool Invert
            {
                get => field;
                set
                {
                    if (field == value)
                    {
                        return;
                    }

                    field = value;

                    if (value)
                    {
                        Start = _end;
                        End = _start;
                        (_start.Color, _end.Color) = (_end.Color, _start.Color);
                        return;
                    }

                    Start = _start;
                    End = _end;
                    (_start.Color, _end.Color) = (_end.Color, _start.Color);
                }
            }

            private GradientStop Start;
            private GradientStop End;

            private readonly GradientStop _start;
            private readonly GradientStop _end;
        }

        #endregion
    }
}

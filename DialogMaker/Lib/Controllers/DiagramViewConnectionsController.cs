using Acly;
using Acly.Numbers;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Editor;
using DialogMaker.Lib.Elements;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DialogMaker.Lib.Controllers
{
    public class DiagramViewConnectionsController : MouseController
    {
        public DiagramViewConnectionsController(DiagramView view, Canvas canvas)
            : base(view)
        {
            View = view;
            Canvas = canvas;
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
                    InvokePropertyChanging(nameof(CurvesThickness));

                    field = value;

                    foreach (var curve in _curves)
                    {
                        curve.Line.StrokeThickness = value;
                    }

                    InvokePropertyChanged(nameof(CurvesThickness));
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
                    InvokePropertyChanging(nameof(CurvesOffset));

                    field = value;

                    foreach (var curve in _curves)
                    {
                        curve.Line.Offset = value;
                    }

                    InvokePropertyChanged(nameof(CurvesOffset));
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
                    InvokePropertyChanging(nameof(CurvesEasing));

                    field = value;

                    foreach (var curve in _curves)
                    {
                        curve.Line.Easing = value;
                    }

                    InvokePropertyChanged(nameof(CurvesEasing));
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
                    InvokePropertyChanging(nameof(CurvesResolution));

                    field = value;

                    foreach (var curve in _curves)
                    {
                        curve.Line.Resolution = value;
                    }

                    InvokePropertyChanged(nameof(CurvesResolution));
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
                    InvokePropertyChanging(nameof(Dialog));
                    field = value;
                    InvokePropertyChanged(nameof(Dialog));
                }
            }
        }

        private readonly ElementsPool<CurveLine> _curvesPool = new();
        private readonly List<Curve> _curves = [];
        private PortSearchResult _currentCurve;

        #region Управление

        public void UpdateConnections(DialogProjectNode? node = null)
        {
            if (Dialog == null)
            {
                return;
            }

            var connections = Dialog.GetPairConnections(node);
            List<Curve>? curvesToRemove = null;
            Action<Curve> handleCurve = EmptyCurveHandler;

            if (node == null)
            {
                curvesToRemove = [.. _curves];
                handleCurve = c => curvesToRemove.Remove(c);
            }

            foreach (var info in connections)
            {
                foreach (var end in info.Value)
                {
                    var curve = GetCurve(info.Key, true);
                    curve.EndPort = end;
                    handleCurve(curve);
                }
            }

            if (curvesToRemove != null)
            {
                foreach (var curve in curvesToRemove)
                {
                    RemoveCurve(curve, true);
                }
            }
        }
        public void UpdatePosition(DialogProjectNode node)
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
        public void RemoveConnections(DialogProjectNode node)
        {
            var ports = node.GetPorts();

            bool ContainsNodePort(Curve curve)
            {
                foreach (var port in ports)
                {
                    if (curve.StartPort == port ||
                        curve.EndPort == port)
                    {
                        return true;
                    }
                }

                return false;
            }

            _curves.RemoveAll(curve =>
            {
                if (ContainsNodePort(curve))
                {
                    RemoveCurve(curve, false);
                    return true;
                }

                return false;
            });
        }
        public void Clear()
        {
            foreach (var curve in _curves)
            {
                RemoveCurve(curve, false);
            }

            _curves.Clear();
        }

        protected override void Dispose(bool isDisposing)
        {
            View.PreviewMouseDown -= OnMouseDown;
            View.PreviewMouseMove -= OnMouseMove;
            View.PreviewMouseUp -= OnMouseUp;

            Clear();
            _curvesPool.Dispose();

            base.Dispose(isDisposing);
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

            Panel.SetZIndex(view, -1);
            view.RemoveFromParent();
            Canvas.Children.Add(view);

            view.StrokeThickness = CurvesThickness;
            view.Resolution = CurvesResolution;
            view.Offset = CurvesOffset;
            view.Easing = CurvesEasing;
            view.Inverse = port.Original is DialogProjectNodeInput;
            result = new(view, Canvas);

            _curves.Add(result);

            result.StartPort = port;

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

        private async Task<PortSearchResult> FindPort(MouseButtonEventArgs mouse)
        {
            PortSearchResult result = new();
            bool touchedSomeElement = false;
            int skipCount = 0;
            Point position = mouse.GetPosition(View);

            await View.Fetch(position, obj =>
            {
                if (obj is DiagramNodePort view &&
                    view.GetConnectorRect(View).IntersectsWith(position) &&
                    view.DataContext is DialogProjectNodePortProxy port)
                {
                    result.Port = port;
                }
                if (obj is ISelectable)
                {
                    touchedSomeElement = true;
                }                
            }, callback =>
            {
                if (!result.IsEmpty || skipCount > 2)
                {
                    return true;
                }
                if (callback.VisualHit is CurveLine)
                {
                    return false;
                }

                skipCount++;

                return false;
            });

            if (result.Port == null && !touchedSomeElement)
            {
                foreach (var curve in _curves)
                {
                    var line = curve.Line;
                    var positionOnCurve = mouse.GetPosition(line);

                    if (line.IsHit(positionOnCurve, line.StrokeThickness * 2))
                    {
                        var positionOnCanvas = mouse.GetPosition(Canvas);
                        var startDistance = curve.StartPosition.Distance(positionOnCanvas);
                        var endDistance = curve.EndPosition.Distance(positionOnCanvas);

                        result.Curve = curve;
                        result.Port = startDistance > endDistance ? curve.EndPort : curve.StartPort;
                        result.IsCurveControl = true;
                        break;
                    }
                }
            }

            return result;
        }

        #endregion

        #region События

        protected override async void OnMouseDown(object? sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed &&
                _currentCurve.IsEmpty)
            {
                var portSearchResult = await FindPort(e);
                var port = portSearchResult.Port;

                if (port != null)
                {
                    e.Handled = true;
                    portSearchResult.Curve ??= GetCurve(port, port.ConnectionsCount > 0 && port.Original.Multiconnection && portSearchResult.Curve == null);

                    _currentCurve = portSearchResult;

                    if (port.Original is DialogProjectNodeInput)
                    {
                        portSearchResult.Curve.Line.DirectEnd = port.Node.Inverted;
                    }
                    else
                    {
                        portSearchResult.Curve.Line.DirectStart = port.Node.Inverted;
                    }

                    portSearchResult.SetPosition(e.GetPosition(Canvas));
                    portSearchResult.Curve.SyncPositions();
                }
            }

            base.OnMouseDown(sender, e);
        }
        protected override void OnMouseMove(object? sender, MouseEventArgs e)
        {
            base.OnMouseMove(sender, e);

            if (_currentCurve.IsEmpty)
            {
                return;
            }

            var position = e.GetPosition(Canvas);
            var curve = _currentCurve.Curve;

            _currentCurve.SetPosition(position);

            if (curve == null)
            {
                return;
            }

            if (curve.StartPort?.Original is DialogProjectNodeInput)
            {
                curve.Line.DirectStart = position.X > curve.StartPosition.X;
            }
            else if (curve.StartPort?.Original is DialogProjectNodeOutput)
            {
                curve.Line.DirectEnd = position.X < curve.StartPosition.X;
            }
        }
        protected override async void OnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            base.OnMouseUp(sender, e);

            if (e.LeftButton != MouseButtonState.Released ||
                _currentCurve.IsEmpty)
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

            var curve = _currentCurve.Curve;

            if (curve == null)
            {
                _currentCurve = default;
                return;
            }

            if (curve.SetConnection(endPort, _currentCurve.IsStartPortControl) == false || endPort == null)
            {
                RemoveCurve(curve);
            }
            else
            {
                curve.SyncPositions();
            }

            _currentCurve = default;
        }

        #endregion

        #region Статика

        private static void EmptyCurveHandler(Curve curve)
        {
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

                    field?.PropertyChanged -= OnPortPropertyChanged;
                    field = value;

                    Line.DirectStart = value?.Node.Inverted == true;

                    if (value == null)
                    {
                        Line.Visibility = Visibility.Collapsed;
                        return;
                    }
                    else
                    {
                        value.PropertyChanged -= OnPortPropertyChanged;
                        value.PropertyChanged += OnPortPropertyChanged;
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

                    field?.PropertyChanged -= OnPortPropertyChanged;
                    field = value;

                    Line.DirectEnd = value?.Node.Inverted == true;

                    if (value == null)
                    {
                        _gradient.EndColor = Colors.White;
                        return;
                    }
                    else
                    {
                        value.PropertyChanged -= OnPortPropertyChanged;
                        value.PropertyChanged += OnPortPropertyChanged;
                    }

                    _gradient.EndColor = value.Color.Color;
                    EndPosition = value.View.GetConnectorPosition(Container);
                }
            }
            public Point StartPosition
            {
                get => Line.StartPoint;
                set
                {
                    Line.StartPoint = value;
                    _gradient.UpdatePosition(value, EndPosition);
                }
            }
            public Point EndPosition
            {
                get => Line.EndPoint;
                set
                {
                    Line.EndPoint = value;
                    _gradient.UpdatePosition(StartPosition, value);
                }
            }

            private readonly Gradient _gradient;

            #region Управление

            public void SyncPositions()
            {
                if (StartPort != null)
                {
                    StartPosition = StartPort.View.GetConnectorPosition(Container);
                    Line.DirectStart = StartPort.Node.Inverted;
                }
                if (EndPort != null)
                {
                    EndPosition = EndPort.View.GetConnectorPosition(Container);
                    Line.DirectEnd = EndPort.Node.Inverted;
                }
            }
            public bool? SetConnection(DialogProjectNodePortProxy? port, bool fromEnd = false)
            {
                if (StartPort == null)
                {
                    return false;
                }
                if (port == null && EndPort != null)
                {
                    StartPort.Original.Disconnect(EndPort.Original);
                }
                else if (port != null)
                {
                    if (port == StartPort && EndPort != null)
                    {
                        return null;
                    }
                    if (StartPort.Original.IsConnected(port.Original))
                    {
                        if (port == EndPort)
                        {
                            return null;
                        }

                        return false;
                    }

                    var sourcePort = StartPort!;
                    var destinationPort = EndPort;

                    if (fromEnd)
                    {
                        (sourcePort, destinationPort) = (destinationPort!, sourcePort);
                    }

                    if (destinationPort != null &&
                        port != EndPort &&
                        sourcePort.Original.IsConnected(destinationPort.Original))
                    {
                        sourcePort.Original.Disconnect(destinationPort.Original);
                    }

                    try
                    {
                        sourcePort.Original.Connect(port.Original);
                    }
                    catch (Exception error)
                    {
                        error.Alert();
                        return false;
                    }
                }

                if (fromEnd)
                {
                    StartPort = port;
                }
                else
                {
                    EndPort = port;
                }

                return true;
            }

            #endregion

            #region События

            private void OnPortPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "Inverted")
                {
                    SyncPositions();
                }
            }

            #endregion
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

                _startPointSetter = p => gradient.StartPoint = p;
                _endPointSetter = p => gradient.EndPoint = p;
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
                _end = gradient.GradientStops[1];

                for (int i = 0; i < gradient.GradientStops.Count - 2; i++)
                {
                    gradient.GradientStops.RemoveAt(gradient.GradientStops.Count - 1);
                }
            }

            public CurveLine Line { get; }
            public Color StartColor
            {
                get => _start.Color;
                set => _start.Color = value;
            }
            public Color EndColor
            {
                get => _end.Color;
                set => _end.Color = value;
            }

            private readonly GradientStop _start;
            private readonly GradientStop _end;
            private readonly Action<Point> _startPointSetter;
            private readonly Action<Point> _endPointSetter;

            public void UpdatePosition(Point start, Point end)
            {
                Rect bounds = Line.RenderedGeometry.Bounds;

                if (bounds.IsEmpty || bounds.Width == 0 || bounds.Height == 0)
                {
                    return;
                }

                double relativeStartX = (start.X - bounds.Left) / bounds.Width;
                double relativeStartY = (start.Y - bounds.Top) / bounds.Height;
                double relativeEndX = (end.X - bounds.Left) / bounds.Width;
                double relativeEndY = (end.Y - bounds.Top) / bounds.Height;

                Point relativeStart = new(relativeStartX, relativeStartY);
                Point relativeEnd = new(relativeEndX, relativeEndY);

                _startPointSetter(relativeStart);
                _endPointSetter(relativeEnd);
            }
        }
        private struct PortSearchResult
        {
            public bool IsEmpty => Port == null && Curve == null;
            public bool IsStartPortControl => Port != null && IsCurveControl && Port == Curve?.StartPort;
            public DialogProjectNodePortProxy? Port;
            public Curve? Curve;
            public bool IsCurveControl;

            public void SetPosition(Point position)
            {
                if (Curve == null)
                {
                    return;
                }
                if (IsStartPortControl)
                {
                    Curve.StartPosition = position;
                    return;
                }

                Curve.EndPosition = position;
            }
        }

        #endregion
    }
}

using Acly.Numbers;
using DialogMaker.Core.Editor;
using DialogMaker.Editor;
using DialogMaker.Lib.Controllers;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DialogMaker.Lib.Elements
{
    public partial class DiagramView : UserControl
    {
        public DiagramView()
        {
            InitializeComponent();

            _canvas.RenderTransform = new TransformGroup()
            {
                Children = [_canvasTranslation, new ScaleTransform()]
            };
            _scaleController = new(_mainGrid)
            {
                Container = _canvas,
                OverrideScaleTransform = _canvas.GetTransform<ScaleTransform>()
            };
            _connections = new(this, _canvas)
            {
                CurvesThickness = CurvesThickness,
                CurvesOffset = CurvesOffset,
                CurvesResolution = CurvesResolution,
                CurvesEasing = CurvesEasing
            };
            _dragAndDrop = new(_connections, _mainGrid);
            _selectionController = new(_dragAndDrop)
            {
                SelectionDepth = 2,
                ExtraMouseButton = MouseButton.Right
            };

            _canvasXDescriptor = DependencyPropertyDescriptor.FromProperty(TranslateTransform.XProperty, typeof(TranslateTransform));
            _canvasYDescriptor = DependencyPropertyDescriptor.FromProperty(TranslateTransform.YProperty, typeof(TranslateTransform));

            _canvasXDescriptor.AddValueChanged(_canvasTranslation, OnCanvasTranslationXChanged);
            _canvasYDescriptor.AddValueChanged(_canvasTranslation, OnCanvasTranslationYChanged);

            _connections.ReleasedOnEmptySpace += OnConnectionsReleasedOnEmptySpace;
            _dragAndDrop.DragUpdated += OnDragAndDropDragUpdated;
            _dragAndDrop.DragCheck += OnDragAndDropDragCheck;
            _scaleController.ScaleChanged += OnScaleControllerScaleChanged;

            _selectionController.EmptyClick += OnSelectionControllerEmptyClick;
            _selectionController.Selected += OnSelectionControllerSelected;
        }
        ~DiagramView()
        {
            Dispatcher.Invoke(() =>
            {
                var dialog = Dialog;

                if (dialog?.CurrentView == this)
                {
                    dialog.CurrentView = null;
                }
            });

            _canvasXDescriptor.RemoveValueChanged(_canvasTranslation, OnCanvasTranslationXChanged);
            _canvasYDescriptor.RemoveValueChanged(_canvasTranslation, OnCanvasTranslationYChanged);
        }

        public event EventHandler<ItemEventArgs<Point>>? PositionChanged;
        public event EventHandler? Redraw;

        public UIElementCollection Children => _canvas.Children;
        public ProjectDialog? Dialog
        {
            get => GetValue(DialogProperty) as ProjectDialog;
            set => SetValue(DialogProperty, value);
        }
        public double CurvesThickness
        {
            get => (double)GetValue(CurvesThicknessProperty);
            set => SetValue(CurvesThicknessProperty, value);
        }
        public double CurvesOffset
        {
            get => (double)GetValue(CurvesOffsetProperty);
            set => SetValue(CurvesOffsetProperty, value);
        }
        public int CurvesResolution
        {
            get => (int)GetValue(CurvesResolutionProperty);
            set => SetValue(CurvesResolutionProperty, value);
        }
        public Easing CurvesEasing
        {
            get => (Easing)GetValue(CurvesEasingProperty);
            set => SetValue(CurvesEasingProperty, value);
        }
        public double MaxZoom
        {
            get => (double)GetValue(MaxZoomProperty);
            set => SetValue(MaxZoomProperty, value);
        }
        public double MinZoom
        {
            get => (double)GetValue(MinZoomProperty);
            set => SetValue(MinZoomProperty, value);
        }
        public Point Position
        {
            get => (Point)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }
        public Canvas Canvas => _canvas;

        private readonly TranslateTransform _canvasTranslation = new();
        private readonly DependencyPropertyDescriptor _canvasXDescriptor;
        private readonly DependencyPropertyDescriptor _canvasYDescriptor;
        private readonly DragAndDropController _dragAndDrop;
        private readonly ViewScaleController _scaleController;
        private readonly MouseMultiselectController _selectionController;
        private readonly DiagramViewConnectionsController _connections;

        #region Управление

        public Size GetActualSize()
        {
            double maxLeft = ActualWidth;
            double maxTop = ActualHeight;

            foreach (UIElement child in _canvas.Children)
            {
                var left = Canvas.GetLeft(child);
                var top = Canvas.GetTop(child);

                if (!double.IsNaN(left))
                {
                    maxLeft = Math.Max(maxLeft, Math.Abs(left));
                }
                if (!double.IsNaN(top))
                {
                    maxTop = Math.Max(maxTop, Math.Abs(top));
                }
            }

            return new(maxLeft, maxTop);
        }
        public Point GetAbsolutePosition()
        {
            return _canvas.TransformToVisual(this.GetWindow()).Transform(new(0, 0));
        }

        public bool TryGetNode(DiagramNode node, [NotNullWhen(true)] out DialogProjectNode? result)
        {
            result = node.Node;
            return result != null;
        }

        private async void SetDialog(ProjectDialog? oldValue, ProjectDialog? newValue)
        {
            if (oldValue == newValue)
            {
                return;
            }
            if (oldValue != null)
            {
                oldValue.Nodes.ItemChanged -= OnNodesItemChanged;

                if (newValue == null && oldValue.CurrentView == this)
                {
                    oldValue.CurrentView = null;
                }

                foreach (var node in oldValue.Nodes)
                {
                    ClearNode(node);
                }
                try
                {
                    ClearConnections();
                }
                catch (Exception error)
                {
                    error.Log();
                }
            }

            _canvas.Children.Clear();

            if (newValue != null)
            {
                newValue.CurrentView = this;

                newValue.Nodes.ItemChanged -= OnNodesItemChanged;
                newValue.Nodes.ItemChanged += OnNodesItemChanged;

                foreach (var node in newValue.Nodes)
                {
                    CreateNode(node);
                }
            }

            ContextMenu = newValue?.EditorContextMenu;

            await Dispatcher.InvokeAsync(() => { });

            int tries = 0;

            while (2 > tries)
            {
                try
                {
                    if (_connections.Dialog != null)
                    {
                        ClearConnections();
                    }

                    _connections.Dialog = newValue;
                    await Task.Delay(10);
                    _connections.UpdateConnections();
                }
                catch (InvalidOperationException invalidTry)
                {
                    tries++;
                    Logger.Log(invalidTry);
                    await Task.Delay(50);
                    continue;
                }
                catch (Exception error)
                {
                    error.Log();
                }

                break;
            }
        }

        private void UpdateCanvasSize()
        {
            double width = RenderSize.Width;
            double height = RenderSize.Height;

            foreach (UIElement child in _canvas.Children)
            {
                if (child.RenderTransform is not TranslateTransform translation)
                {
                    continue;
                }

                width = Math.Max(width, translation.X + child.RenderSize.Width);
                height = Math.Max(height, translation.Y + child.RenderSize.Height);
            }

            _canvas.Width = width;
            _canvas.Height = height;
        }
        private void ClearConnections()
        {
            _connections.Clear();
            _connections.Dialog = null;
        }
        private void CreateNode(DialogProjectNode node)
        {
            var view = node.View;
            view.RemoveFromParent();

            view.Redraw -= OnNodeViewRedraw;
            view.Redraw += OnNodeViewRedraw;

            _canvas.Children.Add(view);
        }

        private void RemoveNode(DialogProjectNode node)
        {
            var view = node.View;

            ClearNode(node);

            _connections.RemoveConnections(node);
            _canvas.Children.Remove(view);
        }
        private void ClearNode(DialogProjectNode node)
        {
            node.View.Redraw -= OnNodeViewRedraw;
        }

        #endregion

        #region События

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);

            var dialog = Dialog;

            if (dialog != null)
            {
                dialog.LastMouseClickPosition = e.GetPosition(_canvas);
            }
        }

        private void OnCanvasTranslationXChanged(object? sender, EventArgs e)
        {
            if (sender is TranslateTransform translation)
            {
                Position = new()
                {
                    X = translation.X,
                    Y = Position.Y
                };
            }
        }
        private void OnCanvasTranslationYChanged(object? sender, EventArgs e)
        {
            if (sender is TranslateTransform translation)
            {
                Position = new()
                {
                    X = Position.X,
                    Y = translation.Y
                };
            }
        }
        private void OnScaleControllerScaleChanged(object? sender, EventArgs e)
        {
            Redraw?.Invoke(this, e);
        }

        private async void OnConnectionsReleasedOnEmptySpace(object? sender, ConnectionReleaseEventArgs e)
        {
            var position = e.Mouse.GetPosition(_canvas);
            await NodeSelectorController.Request(e.Port.Node.Dialog, e.Port, e.Mouse, position);
        }

        private void OnNodeViewRedraw(object? sender, EventArgs e)
        {
            if (sender is DiagramNode view && TryGetNode(view, out var node))
            {
                _connections.UpdatePosition(node);
                Redraw?.Invoke(this, EventArgs.Empty);
            }
        }
        private async void OnNodesItemChanged(object? sender, CollectionItemEventArgs<DialogProjectNode> e)
        {
            if (e.Action == CollectionItemAction.Add)
            {
                CreateNode(e.Item);
                await Task.Delay(50);
                _connections.UpdateConnections(e.Item);
            }
            else if (e.Action == CollectionItemAction.Remove)
            {
                RemoveNode(e.Item);
            }
        }

        private void OnDragAndDropDragUpdated(object? sender, DragEventArgs<List<FrameworkElement>> e)
        {
            UpdateCanvasSize();

            foreach (var element in e.Element)
            {
                if (element is DiagramNode node
                    && TryGetNode(node, out var model))
                {
                    _connections.UpdatePosition(model);
                }
            }
        }
        private void OnDragAndDropDragCheck(object? sender, DragCheckEventArgs e)
        {
            if (e.PotentialDragObject.Equals(this))
            {
                e.Ignore = true;
                return;
            }
            if (e.PotentialDragObject.Equals(_mainGrid))
            {
                e.PotentialDragObject = _canvas;
                e.DragMouseButton = MouseButton.Middle;
                return;
            }

            e.Ignore = e.PotentialDragObject is not DiagramView &&
                       e.PotentialDragObject is not DiagramNode;

            if (e.PotentialDragObject.Equals(_canvas))
            {
                e.DragMouseButton = MouseButton.Middle;
            }
        }
        private void OnSelectionControllerSelected(object? sender, SelectionEventArgs<ISelectable> e)
        {
            var dialog = Dialog;
            var node = e.Item.ToNode();

            if (dialog == null ||
                node == null)
            {
                return;
            }
            if (e.IsSingle && dialog.SelectedNodes.Count > 1)
            {
                dialog.SelectedNodes.Clear();
            }

            if (e.IsSingle && dialog.SelectedNodes.Count > 0)
            {
                dialog.SelectedNodes[0] = node;
            }
            else
            {
                dialog.SelectedNodes.Add(node);
            }
        }

        private void OnSelectionControllerEmptyClick(object? sender, EventArgs e)
        {
            Dialog?.SelectedNodes.Clear();
        }

        private static void OnDialogChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramView view)
            {
                view.SetDialog(e.OldValue as ProjectDialog, e.NewValue as ProjectDialog);
            }
        }
        private static void OnCurvesThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramView view)
            {
                view._connections.CurvesThickness = (double)e.NewValue;
            }
        }
        private static void OnCurvesOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramView view)
            {
                view._connections.CurvesOffset = (double)e.NewValue;
            }
        }
        private static void OnCurvesEasingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramView view)
            {
                view._connections.CurvesEasing = (Easing)e.NewValue;
            }
        }
        private static void OnCurvesResolutionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramView view)
            {
                view._connections.CurvesResolution = (int)e.NewValue;
            }
        }
        private static void OnMaxZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramView view)
            {
                view._scaleController.MaxScale = (double)e.NewValue;
            }
        }
        private static void OnMinZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramView view)
            {
                view._scaleController.MinScale = (double)e.NewValue;
            }
        }
        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramView view && e.NewValue is Point position)
            {
                var translation = view._canvasTranslation;
                translation.X = position.X;
                translation.Y = position.Y;

                view.PositionChanged?.Invoke(d, new(position));
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty DialogProperty = DependencyProperty.Register(nameof(Dialog), typeof(ProjectDialog),
            typeof(DiagramView), new(OnDialogChanged));
        public static readonly DependencyProperty CurvesThicknessProperty = DependencyProperty.Register(nameof(CurvesThickness), typeof(double),
            typeof(DiagramView), new(OnCurvesThicknessChanged));
        public static readonly DependencyProperty CurvesOffsetProperty = DependencyProperty.Register(nameof(CurvesOffset), typeof(double),
            typeof(DiagramView), new(OnCurvesOffsetChanged));
        public static readonly DependencyProperty CurvesEasingProperty = DependencyProperty.Register(nameof(CurvesEasing), typeof(Easing),
            typeof(DiagramView), new(Easing.EaseInOutCubic, OnCurvesEasingChanged));
        public static readonly DependencyProperty CurvesResolutionProperty = DependencyProperty.Register(nameof(CurvesResolution), typeof(int),
            typeof(DiagramView), new(8, OnCurvesResolutionChanged));
        public static readonly DependencyProperty MaxZoomProperty = DependencyProperty.Register(nameof(MaxZoom), typeof(double),
            typeof(DiagramView), new(2d, OnMaxZoomChanged));
        public static readonly DependencyProperty MinZoomProperty = DependencyProperty.Register(nameof(MinZoom), typeof(double),
            typeof(DiagramView), new(0.25d, OnMinZoomChanged));
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(nameof(Position), typeof(Point),
            typeof(DiagramView), new(new Point(0, 0), OnPositionChanged));

        #endregion
    }
}

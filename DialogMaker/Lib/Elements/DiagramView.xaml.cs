using Acly;
using Acly.Numbers;
using DialogMaker.Editor;
using DialogMaker.Lib.Controllers;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DragEventArgs = DialogMaker.Lib.Controllers.DragEventArgs;

namespace DialogMaker.Lib.Elements
{
    public partial class DiagramView : UserControl
    {
        public DiagramView()
        {
            InitializeComponent();

            _canvas.RenderTransform = new TranslateTransform();
            _dragAndDrop = new(this);
            _connections = new(this, _canvas)
            {
                CurvesThickness = CurvesThickness,
                CurvesOffset = CurvesOffset,
                CurvesResolution = CurvesResolution,
                CurvesEasing = CurvesEasing
            }
            ;

            _dragAndDrop.DragUpdated += OnDragAndDropDragUpdated;
            _dragAndDrop.DragCheck += OnDragAndDropDragCheck;
        }

        public event EventHandler<ItemMouseEventArgs<DialogProjectNodePortProxy>>? PortPressed;

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

        private readonly DragAndDropController _dragAndDrop;
        private readonly DiagramViewConnectionsController _connections;

        #region Управление

        public bool TryGetNode(DiagramNode node, [NotNullWhen(true)] out DialogProjectNode? result)
        {
            result = node.Node;
            return result != null;
        }

        private void SetDialog(ProjectDialog? oldValue, ProjectDialog? newValue)
        {
            if (oldValue == newValue)
            {
                return;
            }

            _canvas.Children.Clear();

            if (oldValue != null)
            {
                oldValue.Nodes.ItemChanged -= OnNodesItemChanged;
            }
            if (newValue != null)
            {
                newValue.Nodes.ItemChanged += OnNodesItemChanged;

                foreach (var node in newValue.Nodes)
                {
                    CreateNode(node);
                }
            }

            _connections.Dialog = newValue;

            ContextMenu = newValue?.EditorContextMenu;
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

        private void CreateNode(DialogProjectNode node)
        {
            var view = node.View;
            view.RemoveFromParent();

            view.PortPressed -= OnViewPortPressed;
            view.PortPressed += OnViewPortPressed;

            _canvas.Children.Add(view);
        }
        private void RemoveNode(DialogProjectNode node)
        {
            var view = node.View;
            view.PortPressed -= OnViewPortPressed;

            _canvas.Children.Remove(view);
        }

        #endregion

        #region События

        private void OnViewPortPressed(object? sender, ItemMouseEventArgs<DialogProjectNodePortProxy> e)
        {
            PortPressed?.Invoke(sender, e);
        }

        private void OnNodesItemChanged(object? sender, CollectionItemEventArgs<DialogProjectNode> e)
        {
            if (e.Action == CollectionItemAction.Add)
            {
                CreateNode(e.Item);
            }
            else if (e.Action == CollectionItemAction.Remove)
            {
                RemoveNode(e.Item);
            }
        }

        private void OnDragAndDropDragUpdated(object? sender, DragEventArgs e)
        {
            UpdateCanvasSize();

            if (e.Element is DiagramNode node 
                && TryGetNode(node, out var model))
            {
                _connections.UpdateConnections(model);
            }
        }
        private void OnDragAndDropDragCheck(object? sender, DragCheckEventArgs e)
        {
            if (e.PotentialDragObject.Equals(_mainGrid))
            {
                e.PotentialDragObject = _canvas;
                return;
            }

            e.Ignore = e.PotentialDragObject is not DiagramView &&
                       e.PotentialDragObject is not DiagramNode;
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

        #endregion
    }
}

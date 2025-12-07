using Acly;
using System.Windows;
using System.Windows.Controls;
using DialogMaker.Lib.Controllers;
using DragEventArgs = DialogMaker.Lib.Controllers.DragEventArgs;
using System.Windows.Media;
using DialogMaker.Editor;

namespace DialogMaker.Lib.Elements
{
    public partial class DiagramView : UserControl
    {
        public DiagramView()
        {
            InitializeComponent();

            _dragAndDrop = new(this);
            _dragAndDrop.DragUpdated += OnDragAndDropDragUpdated;
            _dragAndDrop.DragCheck += OnDragAndDropDragCheck;
        }

        public UIElementCollection Children => _canvas.Children;
        public ProjectDialog? Dialog
        {
            get => GetValue(DialogProperty) as ProjectDialog;
            set => SetValue(DialogProperty, value);
        }

        private readonly DragAndDropController _dragAndDrop;
        private readonly ElementsPool<DiagramNode> _nodesPool = new();
        private readonly Dictionary<DialogProjectNode, DiagramNode> _nodes = [];

        #region Управление

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
            if (!_nodes.TryGetValue(node, out var view))
            {
                view = _nodesPool.GetElement();
                view.Node = node;
                _canvas.Children.Add(view);
            }
        }
        private void RemoveNode(DialogProjectNode node)
        {
            if (_nodes.TryGetValue(node, out var view))
            {
                view.Node = null;
                _nodes.Remove(node);
                _canvas.Children.Remove(view);
                _nodesPool.Free(view);
            }
        }

        #endregion

        #region События

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
        }
        private void OnDragAndDropDragCheck(object? sender, DragCheckEventArgs e)
        {
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

        #endregion

        #region Dependency

        public static readonly DependencyProperty DialogProperty = DependencyProperty.Register(nameof(Dialog), typeof(ProjectDialog),
            typeof(DiagramView), new(OnDialogChanged));

        #endregion
    }
}

using DialogMaker.Core.Editor;
using DialogMaker.Editor;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DialogMaker.Lib.Elements
{
    public partial class DialogAndResourcesView : UserControl
    {
        public DialogAndResourcesView()
        {
            InitializeComponent();
            DataContext = _viewModel;
            _diagram.Clip = _diagramClipRectangle;
        }

        public event EventHandler<ItemEventArgs<DiagramView>>? DiagramViewRedraw;

        public ProjectStructureItem? Item
        {
            get => GetValue(ItemProperty) as ProjectStructureItem;
            set => SetValue(ItemProperty, value);
        }
        public ProjectResources? ItemResources
        {
            get => GetValue(ItemResourcesProperty) as ProjectResources;
            set => SetValue(ItemResourcesProperty, value);
        }

        private ProjectDialog? Dialog
        {
            get => _viewModel.Dialog;
            set => _viewModel.Dialog = value;
        }

        private readonly RectangleGeometry _diagramClipRectangle = new();
        private readonly DialogAndResourcesViewModel _viewModel = new();

        #region Управление

        public Rect GetDiagramRect()
        {
            Point position = new();

            try
            {
                position = TransformToVisual(this.GetWindow()).Transform(new(0, 0));
            }
            catch (Exception error)
            {
                Logger.Log(error);
            }

            return new(position, _diagramSizeReference.RenderSize);
        }

        private void UpdateDiagramClipping()
        {
            Point position;

            try
            {
                position = TransformToVisual(this.GetWindow()).Transform(new(0, 0));
            }
            catch (Exception error)
            {
                Logger.Log(error);
                return;
            }

            _diagram.Margin = new(-position.X, -position.Y, 0, 0);
            _diagramClipRectangle.Rect = new(position, _diagramSizeReference.RenderSize);
        }

        #endregion

        #region События

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateDiagramClipping();
        }

        private void OnDiagramRedraw(object sender, EventArgs e)
        {
            UpdateDiagramClipping();
            DiagramViewRedraw?.Invoke(this, new(_diagram));
        }
        private void OnDiagramSizeChanged(object sender, SizeChangedEventArgs e)
        {
            OnDiagramRedraw(this, e);
        }

        private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DialogAndResourcesView view)
            {
                return;
            }
            if (e.NewValue is ProjectDialog dialog)
            {
                view.Dialog = dialog;
            }
            else if (e.NewValue is DialogProjectNode node)
            {
                view.Dialog = node.Dialog;
            }
            else
            {
                view.Dialog = null;
            }
        }
        private static void OnResourcesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DialogAndResourcesView view)
            {
                view._viewModel.Resources = e.NewValue as ProjectResources;
                view._viewModel.Project = view._viewModel.Resources?.Controller;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(ProjectStructureItem),
            typeof(DialogAndResourcesView), new(OnItemChanged));
        public static readonly DependencyProperty ItemResourcesProperty = DependencyProperty.Register(nameof(ItemResources), typeof(ProjectResources),
            typeof(DialogAndResourcesView), new(OnResourcesChanged));

        #endregion
    }
}

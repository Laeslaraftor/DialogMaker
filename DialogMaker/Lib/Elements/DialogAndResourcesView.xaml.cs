using DialogMaker.Editor;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class DialogAndResourcesView : UserControl
    {
        public DialogAndResourcesView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

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

        private readonly DialogAndResourcesViewModel _viewModel = new();

        #region События

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

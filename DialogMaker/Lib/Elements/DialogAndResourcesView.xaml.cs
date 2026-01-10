using DialogMaker.Core.Editor;
using DialogMaker.Core.Executioning;
using DialogMaker.Editor;
using System.ComponentModel;
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
            set
            {
                if (_viewModel.Dialog != value)
                {
                    if (_viewModel.Dialog != null)
                    {
                        _viewModel.Dialog.Original.PropertyChanged -= OnOriginalDialogPropertyChanged;
                    }

                    _viewModel.Dialog = value;

                    if (value == null)
                    {
                        _viewModel.Structure = null;
                    }
                    else
                    {
                        value.Original.PropertyChanged += OnOriginalDialogPropertyChanged;
                        UpdateStructure(value.Original);
                    }
                }
            }
        }

        

        private readonly DialogAndResourcesViewModel _viewModel = new();

        #region Управление

        private void UpdateStructure(DialogProjectDialog dialog)
        {
            try
            {
                _viewModel.Structure = DialogActionsMap.CreateStructure(dialog);
            }
            catch (Exception error)
            {
                error.Alert();
            }
        }

        #endregion

        #region События

        private void OnOriginalDialogPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Nodes" && sender is DialogProjectDialog dialog)
            {
                UpdateStructure(dialog);
            }
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

using DialogMaker.Core.Executioning;
using DialogMaker.Editor;
using DialogMaker.Lib.Data;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class DiagramStructureView : UserControl
    {
        public DiagramStructureView()
        {
            InitializeComponent();
            _segmentsList.ItemsSource = _sections;
            CanUpdateStructure = true;
        }

        public ProjectDialog? Dialog
        {
            get => GetValue(DialogProperty) as ProjectDialog;
            set => SetValue(DialogProperty, value);
        }

        private bool CanUpdateStructure
        {
            get => Dialog != null && field;
            set
            {
                field = value;
                _updateStructureButton.IsEnabled = CanUpdateStructure;
            }
        }

        private readonly ObservableCollection<DialogStructureSection> _sections = [];

        #region Управление

        private async Task UpdateStructure()
        {
            var dialog = Dialog;

            _sections.Clear();

            if (dialog == null)
            {
                return;
            }

            var structure = await Task.Run(() =>
            {
                return DialogActionsMap.CreateStructure(dialog.Original);
            });

            foreach (var section in structure.Sections)
            {
                _sections.Add(new(dialog, structure, section));
            }
        }

        #endregion

        #region События

        private async void OnUpdateStructureButtonClicked(object sender, RoutedEventArgs e)
        {
            CanUpdateStructure = false;

            try
            {
                await UpdateStructure();
            }
            catch (Exception error)
            {
                error.Alert();
            }

            CanUpdateStructure = true;
        }

        private static void OnDialogChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramStructureView view)
            {
                view.CanUpdateStructure = view.CanUpdateStructure;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty DialogProperty = DependencyProperty.Register(nameof(Dialog), typeof(ProjectDialog),
            typeof(DiagramStructureView), new(OnDialogChanged));

        #endregion
    }
}

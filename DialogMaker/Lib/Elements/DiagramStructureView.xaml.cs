using DialogMaker.Core.Executioning.Debugging;
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
        }

        public DialogCodeStructure? Structure
        {
            get => GetValue(StructureProperty) as DialogCodeStructure;
            set => SetValue(StructureProperty, value);
        }
        public ProjectDialog? Dialog
        {
            get => GetValue(DialogProperty) as ProjectDialog;
            set => SetValue(DialogProperty, value);
        }

        private readonly ObservableCollection<DialogStructureSection> _sections = [];

        #region Управление

        private void UpdateStructure(DialogCodeStructure? structure, ProjectDialog? dialog)
        {
            _sections.Clear();

            if (structure == null || dialog == null)
            {
                return;
            }

            foreach (var section in structure.Sections)
            {
                _sections.Add(new(dialog, structure, section));
            }
        }

        #endregion

        #region События

        private static void OnStructureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramStructureView view)
            {
                view.UpdateStructure(e.NewValue as DialogCodeStructure, view.Dialog);
            }
        }
        private static void OnDialogChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramStructureView view)
            {
                view.UpdateStructure(view.Structure, e.NewValue as ProjectDialog);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty StructureProperty = DependencyProperty.Register(nameof(Structure), typeof(DialogCodeStructure),
            typeof(DiagramStructureView), new(OnStructureChanged));
        public static readonly DependencyProperty DialogProperty = DependencyProperty.Register(nameof(Dialog), typeof(ProjectDialog),
            typeof(DiagramStructureView), new(OnDialogChanged));

        #endregion
    }
}

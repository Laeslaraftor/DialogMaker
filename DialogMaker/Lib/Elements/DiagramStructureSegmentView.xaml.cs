using DialogMaker.Lib.Data;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class DiagramStructureSegmentView : UserControl
    {
        public DiagramStructureSegmentView()
        {
            InitializeComponent();
            _itemsList.ItemsSource = _items;
        }

        public DialogStructureSection? Section
        {
            get => GetValue(SectionProperty) as DialogStructureSection;
            set => SetValue(SectionProperty, value);
        }

        private readonly ObservableCollection<DialogStructureItem> _items = [];

        #region Управление

        private void SetSection(DialogStructureSection? oldValue, DialogStructureSection? newValue)
        {
            _items.Clear();

            if (newValue == null)
            {
                return;
            }

            _title.Text = $"Сегмент {newValue.Index}";

            foreach (var item in newValue.Section.Items.Values)
            {
                _items.Add(new(newValue, item));
            }
        }

        #endregion

        #region События

        private static void OnSectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramStructureSegmentView view)
            {
                view.SetSection(e.OldValue as DialogStructureSection, e.NewValue as DialogStructureSection);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty SectionProperty = DependencyProperty.Register(nameof(Section), typeof(DialogStructureSection),
            typeof(DiagramStructureSegmentView), new(OnSectionChanged));

        #endregion
    }
}

using DialogMaker.Lib.Data;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class DiagramStructureItemView : UserControl
    {
        public DiagramStructureItemView()
        {
            InitializeComponent();
        }

        public DialogStructureItem? Item
        {
            get => GetValue(ItemProperty) as DialogStructureItem;
            set => SetValue(ItemProperty, value);
        }

        #region Управление

        private void SetItem(DialogStructureItem? oldValue, DialogStructureItem? newValue)
        {
            _node.DataContext = newValue?.Node;
            _dataList.ItemsSource = newValue?.DataNodes;
            _dataList.Visibility = newValue?.DataNodes == null ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion

        #region События

        private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramStructureItemView view)
            {
                view.SetItem(e.OldValue as DialogStructureItem, e.NewValue as DialogStructureItem);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(DialogStructureItem),
            typeof(DiagramStructureItemView), new(OnItemChanged));

        #endregion
    }
}

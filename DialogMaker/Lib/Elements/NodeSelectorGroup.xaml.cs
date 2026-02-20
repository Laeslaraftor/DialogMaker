using DialogMaker.Core.Editor;
using DialogMaker.Lib.Data;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class NodeSelectorGroup : UserControl
    {
        public NodeSelectorGroup()
        {
            InitializeComponent();
        }

        public event EventHandler<ItemEventArgs<NodeSelectorItemInfo>>? ItemSelected;

        public NodeSelectorItemInfo? RootItem
        {
            get => GetValue(RootItemProperty) as NodeSelectorItemInfo;
            set => SetValue(RootItemProperty, value);
        }

        private bool IsMinimized
        {
            get => _itemsList.Visibility == Visibility.Collapsed;
            set
            {
                Visibility visibility = Visibility.Visible;
                double rotation = 90;

                if (value)
                {
                    visibility = Visibility.Collapsed;
                    rotation = 0;
                }

                _itemsList.Visibility = visibility;
                _minimizeButtonRotation.Angle = rotation;
                RootItem?.IsMinimized = value;
            }
        }

        #region Управление

        private void SetRootItem(NodeSelectorItemInfo? oldValue, NodeSelectorItemInfo? newValue)
        {
            if (Equals(oldValue, newValue))
            {
                return;
            }

            oldValue?.PropertyChanged -= OnNodeSelectorGroupPropertyChanged;
            newValue?.PropertyChanged += OnNodeSelectorGroupPropertyChanged;

            _mainGrid.DataContext = newValue;
            IsMinimized = newValue?.IsMinimized == true;
        }

        #endregion

        #region События

        private void OnNodeSelectorGroupPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is NodeSelectorItemInfo item &&
                e.PropertyName == nameof(IsMinimized))
            {
                IsMinimized = item.IsMinimized;
            }
        }

        private void OnMinimizeButtonClicked(object sender, RoutedEventArgs e)
        {
            IsMinimized = !IsMinimized;
        }
        private void OnSelectButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is NodeSelectorItemInfo info)
            {
                ItemSelected?.Invoke(this, new(info));
            }
        }
        private void OnChildNodeSelectorGroupItemSelected(object sender, ItemEventArgs<NodeSelectorItemInfo> e)
        {
            ItemSelected?.Invoke(this, e);
        }

        private static void OnRootItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NodeSelectorGroup view)
            {
                view.SetRootItem(e.OldValue as NodeSelectorItemInfo, e.NewValue as NodeSelectorItemInfo);                
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty RootItemProperty = DependencyProperty.Register(nameof(RootItem), typeof(NodeSelectorItemInfo),
            typeof(NodeSelectorGroup), new(OnRootItemChanged));

        #endregion
    }
}

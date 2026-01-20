using Acly;
using DialogMaker.Lib.Controllers;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class ItemTabsView : UserControl
    {
        public ItemTabsView()
        {
            InitializeComponent();
            Items = [];
            Items.ItemChanged += OnItemsItemChanged;

            _tabControl.ItemsSource = Items;
        }

        public event EventHandler<ValueChangedEventArgs<IItemTab?>>? CurrentItemChanged;

        public EditableCollection<IItemTab> Items
        {
            get => (EditableCollection<IItemTab>)GetValue(ItemsProperty.DependencyProperty);
            private set => SetValue(ItemsProperty, value);
        }
        public IItemTab? CurrentItem
        {
            get => GetValue(CurrentItemProperty) as IItemTab;
            set => SetValue(CurrentItemProperty, value);
        }

        #region Управление

        private void Remove(IItemTab item, EventArgs e)
        {
            if (Items.Remove(item))
            {
                item.OnClosed(this, e);
            }
        }

        #endregion

        #region События

        private void OnCloseViewClicked(object sender, ParameterRoutedEventArgs e)
        {
            if (e.Parameter is IItemTab itemTab)
            {
                Remove(itemTab, e.RoutedEventArgs);
            }
        }
        private void OnTabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems)
            {
                if (item is IItemTab itemTab)
                {
                    itemTab.OnHided(this, e);
                }
            }
            foreach (var item in e.AddedItems)
            {
                if (item is IItemTab itemTab)
                {
                    itemTab.OnShowed(this, e);
                }
            }
        }

        private void OnItemsItemChanged(object? sender, CollectionItemEventArgs<IItemTab> e)
        {
            if (e.Action == CollectionItemAction.Add)
            {
                e.Item.CloseRequested += OnItemCloseRequested;
            }
            else if (e.Action == CollectionItemAction.Remove)
            {
                e.Item.CloseRequested -= OnItemCloseRequested;
            }
        }
        private void OnItemCloseRequested(object? sender, EventArgs e)
        {
            if (sender is IItemTab itemTab)
            {
                Remove(itemTab, e);
            }
        }

        private static void OnCurrentItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ItemTabsView view)
            {
                if (e.NewValue is IItemTab itemTab)
                {
                    int index = view.Items.IndexOf(itemTab);

                    if (index == -1)
                    {
                        view.Items.Add(itemTab);
                        index = view.Items.Count - 1;
                    }

                    view._tabControl.SelectedIndex = index;
                }

                view.CurrentItemChanged?.Invoke(d, new(e));
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyPropertyKey ItemsProperty = DependencyProperty.RegisterReadOnly(nameof(Items), typeof(EditableCollection<IItemTab>),
            typeof(ItemTabsView), new());
        public static readonly DependencyProperty CurrentItemProperty = DependencyProperty.Register(nameof(CurrentItem), typeof(IItemTab),
            typeof(ItemTabsView), new(OnCurrentItemChanged));

        #endregion
    }
}

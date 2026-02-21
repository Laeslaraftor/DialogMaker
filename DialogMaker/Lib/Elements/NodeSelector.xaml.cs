using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Lib.Data;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Lib.Elements
{
    public partial class NodeSelector : UserControl
    {
        public NodeSelector()
        {
            InitializeComponent();

            foreach (var item in CreateItems())
            {
                _items.Add(item.Copy());
            }

            Search(null);

            _groupsList.ItemsSource = _items;
        }

        public event EventHandler<ItemEventArgs<NodeSelectorItemInfo>>? NodeSelected;

        public string? SearchValue
        {
            get => GetValue(SearchValueProperty) as string;
            set => SetValue(SearchValueProperty, value);
        }
        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        private readonly ObservableCollection<NodeSelectorItemInfo> _items = [];

        #region Управление

        private void Search(string? value)
        {
            Predicate<NodeSelectorItemInfo> itemPredicate = i => true;

            if (value != null)
            {
                itemPredicate = i => i.Name?.Contains(value, StringComparison.InvariantCultureIgnoreCase) == true;
            }

            FilterItems(itemPredicate);
        }

        private void FilterItems(Predicate<NodeSelectorItemInfo> predicate)
        {
            bool firstNotSelected = true;

            void InnerFilterItems(NodeSelectorItemInfo root)
            {
                if (root.Children == null)
                {
                    root.IsEnabled = false;
                    return;
                }

                bool allDisabled = true;

                foreach (var item in root.Children)
                {
                    if (item.IsContainer)
                    {
                        if (item.Children != null)
                        {
                            InnerFilterItems(item);
                        }
                        else
                        {
                            item.IsEnabled = false;
                        }
                    }
                    else
                    {
                        item.IsEnabled = predicate(item);

                        if (item.IsEnabled && firstNotSelected)
                        {
                            firstNotSelected = false;
                            item.IsSelected = true;
                        }
                        else
                        {
                            item.IsSelected = false;
                        }
                    }

                    if (item.IsEnabled)
                    {
                        allDisabled = false;
                    }
                }

                root.IsEnabled = !allDisabled;
                root.IsMinimized = allDisabled;
            }

            foreach (var item in _items)
            {
                InnerFilterItems(item);
            }
        }

        private NodeSelectorItemInfo? SelectNext()
        {
            NodeSelectorItemInfo? selectedItem = null;
            NodeSelectorItemInfo? result = null;

            ForEachItems(_items, item =>
            {
                if (item.IsSelected)
                {
                    selectedItem = item;
                    return false;
                }
                if (selectedItem != null && item.IsEnabled)
                {
                    selectedItem.IsSelected = false;
                    item.IsSelected = true;
                    result = item;
                    return true;
                }

                return false;
            });

            return result;
        }
        private NodeSelectorItemInfo? SelectPrevious()
        {
            NodeSelectorItemInfo? previousItem = null;
            NodeSelectorItemInfo? result = null;

            ForEachItems(_items, item =>
            {
                if (item.IsSelected)
                {
                    if (previousItem != null)
                    {
                        previousItem.IsSelected = true;
                        item.IsSelected = false;
                        result = previousItem;
                    }
                    
                    return true;
                }
                if (item.IsEnabled)
                {
                    previousItem = item;
                }                

                return false;
            });

            return result;
        }

        #endregion

        #region События

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            bool handled = true;

            if (e.Key == Key.Down)
            {
                SelectNext()?.RequestBringToView();
            }
            else if (e.Key == Key.Up)
            {
                SelectPrevious()?.RequestBringToView();
            }
            else if (e.Key == Key.Enter)
            {
                ForEachItems(_items, item =>
                {
                    if (item.IsSelected)
                    {
                        NodeSelected?.Invoke(this, new(item));
                        return true;
                    }

                    return false;
                });
            }
            else
            {
                handled = false;
            }

            e.Handled = handled;
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchValue = _searchValueEntry.Text;
        }
        private void OnNodeSelectorGroupItemSelected(object sender, ItemEventArgs<NodeSelectorItemInfo> e)
        {
            NodeSelected?.Invoke(this, e);
        }

        private static void OnSearchValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NodeSelector view)
            {
                string? searchValue = e.NewValue?.ToString();
                view.Search(searchValue);
                view._searchValueEntry.Text = searchValue ?? string.Empty;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty SearchValueProperty = DependencyProperty.Register(nameof(SearchValue), typeof(string),
            typeof(NodeSelector), new(OnSearchValueChanged));
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof(SelectedIndex), typeof(int),
            typeof(NodeSelector), new(-1, OnSearchValueChanged));

        #endregion

        #region Статика

        private static ReadOnlyCollection<NodeSelectorItemInfo>? _nodeItems;

        private static ReadOnlyCollection<NodeSelectorItemInfo> CreateItems()
        {
            if (_nodeItems != null)
            {
                return _nodeItems;
            }

            Dictionary<string, NodeSelectorItemInfo> groups = [];
            List<NodeSelectorItemInfo> result = [];
            HashSet<string> paths = [];

            foreach (var info in DialogProjectDialogNode.AvailableNodes.Values)
            {
                paths.Add(info.Path);

                if (!groups.TryGetValue(info.Path, out var group))
                {
                    group = new()
                    {
                        IsContainer = true,
                        Name = info.Path.Split('/')[^1],
                        Children = []
                    };
                    groups.Add(info.Path, group);
                }

                group.Children?.Add(new()
                {
                    Name = info.Metadata.Name,
                    Value = info
                });
            }

            NodeSelectorItemInfo? AddChilds(NodeSelectorItemInfo? parent, string[] pathParts, int currentIndex)
            {
                if (currentIndex >= pathParts.Length)
                {
                    return null;
                }

                string path = string.Empty;

                for (int i = 0; i < currentIndex + 1; i++)
                {
                    if (path != string.Empty)
                    {
                        path += '/';
                    }

                    path += pathParts[i];
                }

                if (groups.TryGetValue(path, out var group))
                {
                    parent?.Children?.Add(group);
                    parent = group;
                }

                AddChilds(parent, pathParts, currentIndex + 1);

                return parent;
            }

            Dictionary<string, string> rootMaxPaths = [];

            foreach (var path in paths)
            {
                var parts = path.Split('/');

                if (!rootMaxPaths.TryGetValue(parts[0], out var maxValue))
                {
                    rootMaxPaths.Add(parts[0], path);
                    continue;
                }
                if (path.Length > maxValue.Length)
                {
                    rootMaxPaths[parts[0]] = path;
                }
            }
            foreach (var path in rootMaxPaths.Values)
            {
                var item = AddChilds(null, path.Split('/'), 0);

                if (item != null)
                {
                    result.Add(item);
                }
            }

            _nodeItems = new(result);

            return _nodeItems;
        }

        private static void ForEachItems(IEnumerable<NodeSelectorItemInfo> items, Predicate<NodeSelectorItemInfo> handler)
        {
            bool stop = false;

            void HandleItems(NodeSelectorItemInfo item)
            {
                if (item.Children == null || stop)
                {
                    return;
                }

                foreach (var child in item.Children)
                {
                    if (child.IsContainer)
                    {
                        HandleItems(child);
                        continue;
                    }

                    stop = handler(child);

                    if (stop)
                    {
                        return;
                    }
                }
            }

            foreach (var item in items)
            {
                HandleItems(item);

                if (stop)
                {
                    return;
                }
            }
        }

        #endregion
    }
}

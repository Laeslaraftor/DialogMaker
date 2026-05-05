using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Lib.Data;
using System.Collections.ObjectModel;
using System.Reflection;
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

            foreach (var item in CreateDefaultItems())
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
        public NodeSelectionMode Mode
        {
            get => (NodeSelectionMode)GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }
        public DialogNodeConnectionType PortsType
        {
            get => (DialogNodeConnectionType)GetValue(PortsTypeProperty);
            set => SetValue(PortsTypeProperty, value);
        }

        private readonly ObservableCollection<NodeSelectorItemInfo> _items = [];

        #region Управление

        public void FocusSearch()
        {
            _searchValueEntry.TextBox.Focus();
        }

        private void Search(string? value)
        {
            var mode = Mode;
            var portsType = PortsType;
            Predicate<NodeSelectorItemInfo> itemPredicate = i => true;
            Predicate<NodeSelectorItemInfo> portPredicate = i => i.Port == null;

            if (value != null)
            {
                itemPredicate = i => i.Name?.Contains(value, StringComparison.InvariantCultureIgnoreCase) == true;
            }
            if (mode != NodeSelectionMode.Default)
            {
                GetPortTypes(mode == NodeSelectionMode.Input, out var modeType, out var dataType);
                portPredicate = i =>
                {
                    if (i.Port == null)
                    {
                        return false;
                    }

                    var property = i.Port.Value.Key;
                    var propertyIsAction = property.PropertyType == dataType;
                    bool isAssignable = property.PropertyType.IsAssignableTo(modeType);

                    return isAssignable && propertyIsAction ^ portsType == DialogNodeConnectionType.Data;
                };
            }

            FilterItems(n => itemPredicate(n) && portPredicate(n));
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

            //e.Handled = handled;
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
        private static void OnFilterValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NodeSelector view)
            {
                view.Search(view.SearchValue);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty SearchValueProperty = DependencyProperty.Register(nameof(SearchValue), typeof(string),
            typeof(NodeSelector), new(OnSearchValueChanged));
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof(SelectedIndex), typeof(int),
            typeof(NodeSelector), new(-1, OnSearchValueChanged));
        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(nameof(Mode), typeof(NodeSelectionMode),
            typeof(NodeSelector), new(NodeSelectionMode.Default, OnFilterValueChanged));
        public static readonly DependencyProperty PortsTypeProperty = DependencyProperty.Register(nameof(PortsType), typeof(DialogNodeConnectionType),
            typeof(NodeSelector), new(DialogNodeConnectionType.Action, OnFilterValueChanged));

        #endregion

        #region Статика

        private static readonly Type _inputActionPort = typeof(DialogProjectNodeInputAction);
        private static readonly Type _inputPort = typeof(DialogProjectNodeInput);
        private static readonly Type _outputActionPort = typeof(DialogProjectNodeOutputAction);
        private static readonly Type _outputPort = typeof(DialogProjectNodeOutput);
        private static ReadOnlyCollection<NodeSelectorItemInfo>? _defaultNodeItems;

        private static ReadOnlyCollection<NodeSelectorItemInfo> CreateDefaultItems()
        {
            _defaultNodeItems ??= CreateItems((infos, node) =>
            {
                infos.Add(new()
                {
                    Name = node.Metadata.Name,
                    Value = node
                });

                CreatePortItems(infos, node, node.Inputs);
                CreatePortItems(infos, node, node.Outputs);
            });

            return _defaultNodeItems;
        }
        private static void CreatePortItems(EditableCollection<NodeSelectorItemInfo> infos, DialogNodeInfo node, ReadOnlyDictionary<PropertyInfo, DialogProjectNodeMetadata> ports)
        {
            foreach (var portInfo in ports)
            {
                infos.Add(new()
                {
                    Name = $"{node.Metadata.Name} → {portInfo.Value.Name}",
                    Value = node,
                    Port = portInfo
                });
            }
        }

        private static ReadOnlyCollection<NodeSelectorItemInfo> CreateItems(Action<EditableCollection<NodeSelectorItemInfo>, DialogNodeInfo> infoHandler)
        {
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

                if (group.Children != null)
                {
                    infoHandler(group.Children, info);
                }
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

            return new(result);
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
        private static void GetPortTypes(bool isInput, out Type modeType, out Type dataType)
        {
            modeType = isInput ? _inputPort : _outputPort;
            dataType = isInput ? _inputActionPort : _outputActionPort;
        }

        #endregion
    }
}

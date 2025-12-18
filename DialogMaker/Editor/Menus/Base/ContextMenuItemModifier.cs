using DialogMaker.Core;
using DialogMaker.Lib;
using DialogMaker.Lib.Elements;
using System.Windows.Controls;
using System.Windows;

namespace DialogMaker.Editor.Menus
{
    public class ContextMenuItemModifier : Disposable
    {
        protected ContextMenuItemModifier(string name)
        {
            Name = name;
        }
        protected ContextMenuItemModifier(string? icon, string name)
            : this(name)
        {
            Icon = icon;
        }

        public string? Icon
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Icon));
                    field = value;
                    InvokePropertyChanged(nameof(Icon));
                }
            }
        }
        public string Name
        {
            get => field ?? string.Empty;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Name));
                    field = value;
                    InvokePropertyChanged(nameof(Name));
                }
            }
        }

        private readonly ElementsPool<MenuItem> _itemsPool = new();
        private readonly Dictionary<ItemCollection, ItemInfo> _items = [];

        #region Управление

        protected bool Clear(ItemCollection items)
        {
            if (_items.TryGetValue(items, out var info))
            {
                _itemsPool.Free(info.Item);
                _items.Remove(items);

                info.ContextMenu.Opened -= OnContextMenuOpened;
                info.ContextMenu.Closed -= OnContextMenuClosed;

                return true;
            }

            return false;
        }
        protected MenuItem GetItem(ContextMenu menu, ItemCollection items)
        {
            if (!_items.TryGetValue(items, out var info))
            {
                var item = _itemsPool.GetElement();

                SetupItem(item);

                menu.Opened -= OnContextMenuOpened;
                menu.Opened += OnContextMenuOpened;
                menu.Closed -= OnContextMenuClosed;
                menu.Closed += OnContextMenuClosed;

                info = new(menu, item);
                _items.Add(items, info);
            }

            return info.Item;
        }

        protected virtual void SetupItem(MenuItem item)
        {
            item.Header = Name;

            if (Icon == null)
            {
                item.Icon = null;
                return;
            }

            if (Icons.TryCreateIconBlock(Icon, item.Icon as TextBlock, out var iconBlock))
            {
                iconBlock.FontSize = 14;
            }

            item.Icon = iconBlock;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            List<ItemCollection> items = [.. _items.Keys];

            foreach (var item in items)
            {
                Clear(item);
            }

            _itemsPool.Dispose();
        }

        #endregion

        #region События

        protected virtual void OnContextMenuOpened(ContextMenu menu, ItemCollection items, MenuItem item)
        {
        }
        protected virtual void OnContextMenuClosed(ContextMenu menu, ItemCollection items, MenuItem item)
        {
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            foreach (var info in _items.Values)
            {
                SetupItem(info.Item);
            }
        }

        private void OnContextMenuOpened(object sender, RoutedEventArgs e)
        {
            if (sender is not ContextMenu menu)
            {
                return;
            }

            foreach (var info in _items)
            {
                if (info.Value.ContextMenu == menu)
                {
                    OnContextMenuOpened(menu, info.Key, info.Value.Item);
                    return;
                }
            }
        }
        private void OnContextMenuClosed(object sender, RoutedEventArgs e)
        {
            if (sender is not ContextMenu menu)
            {
                return;
            }

            foreach (var info in _items)
            {
                if (info.Value.ContextMenu == menu)
                {
                    OnContextMenuClosed(menu, info.Key, info.Value.Item);
                    return;
                }
            }
        }

        #endregion

        #region Классы

        private readonly struct ItemInfo(ContextMenu menu, MenuItem item)
        {
            public ContextMenu ContextMenu { get; } = menu;
            public MenuItem Item { get; } = item;
        }

        #endregion
    }
}

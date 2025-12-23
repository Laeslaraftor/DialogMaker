using DialogMaker.Core;
using DialogMaker.Editor;
using DialogMaker.Lib.Elements;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DialogMaker.Lib.Controllers
{
    public class DialogTabsController(TabControl tabs) : Disposable
    {
        public TabControl TabControl { get; } = tabs;

        private readonly ElementsPool<DialogAndResourcesView> _viewsPool = new();
        private readonly ElementsPool<CloseView> _closeViewsPool = new();
        private readonly ElementsPool<TabItem> _tabItemsPool = new();
        private readonly Dictionary<ProjectStructureItem, TabInfo> _closeViews = [];

        #region Управление

        public void AddItem(ProjectStructureItem item)
        {
            if (_closeViews.TryGetValue(item, out var info))
            {
                info.Select();
                return;
            }

            var view = _viewsPool.GetElement();
            var closeView = _closeViewsPool.GetElement();
            var tabItem = _tabItemsPool.GetElement();
            info = new(TabControl, tabItem, closeView, view, item);

            CreateBinding(closeView, item);

            info.CloseRequested += OnInfoCloseRequested;

            _closeViews.Add(item, info);

            info.Select();
        }
        public bool RemoveItem(ProjectStructureItem item)
        {
            if (_closeViews.TryGetValue(item, out var info))
            {
                return Remove(info);
            }

            return false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            foreach (var info in _closeViews.Values)
            {
                Remove(info, false);
            }

            _closeViews.Clear();
        }

        private bool Remove(TabInfo info, bool full = true)
        {
            if (full && !_closeViews.Remove(info.Item))
            {
                return false;
            }

            info.CloseRequested -= OnInfoCloseRequested;
            info.Dispose();

            BindingOperations.ClearBinding(info.CloseView, CloseView.TitleProperty);

            _viewsPool.Free(info.Content);
            _closeViewsPool.Free(info.CloseView);
            _tabItemsPool.Free(info.TabItem);

            return true;
        }
        private void CreateBinding(CloseView view, ProjectStructureItem item)
        {
            Binding binding = new("Name")
            {
                Mode = BindingMode.TwoWay,
                Source = item
            };

            BindingOperations.SetBinding(view, CloseView.TitleProperty, binding);
        }

        #endregion

        #region События

        private void OnInfoCloseRequested(object? sender, EventArgs e)
        {
            if (sender is TabInfo info)
            {
                Remove(info);
            }
        }

        #endregion

        #region Классы

        private class TabInfo : Disposable
        {
            public TabInfo(TabControl tabControl, TabItem tabItem, CloseView closeView, DialogAndResourcesView content, ProjectStructureItem item)
            {
                TabControl = tabControl;
                TabItem = tabItem;
                Content = content;
                Item = item;
                CloseView = closeView;

                content.Item = item;
                content.ItemResources = item.Resources;
                closeView.Title = item.Name;

                tabItem.Header = closeView;
                tabItem.Content = content;

                tabControl.Items.Add(tabItem);

                closeView.Click += OnCloseViewClick;
            }

            public event EventHandler? CloseRequested;

            public TabControl TabControl { get; }
            public TabItem TabItem { get; }
            public CloseView CloseView { get; }
            public DialogAndResourcesView Content { get; }
            public ProjectStructureItem Item { get; }

            #region Управление

            public void Select()
            {
                TabControl.SelectedIndex = TabControl.Items.IndexOf(TabItem);
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                CloseView.Click -= OnCloseViewClick;

                TabControl.Items.Remove(TabItem);
                TabItem.Header = null;
                TabItem.Content = null;
            }

            #endregion

            #region События

            private void OnCloseViewClick(object? sender, RoutedEventArgs e)
            {
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }

            #endregion
        }

        #endregion
    }
}

using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Editor;
using DialogMaker.Lib.Data;
using DialogMaker.Lib.Elements;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DialogMaker.Lib.Controllers
{
    public class NodeSelectorController : Disposable
    {
        public NodeSelectorController(FrameworkElement owner, NodeSelector nodeSelector, bool isGlobal = true)
        {
            IsGlobal = isGlobal;
            Owner = owner;
            NodeSelector = nodeSelector;
            _translation = nodeSelector.GetTransform<TranslateTransform>();

            nodeSelector.LostFocus += OnNodeSelectorLostFocus;

            if (isGlobal)
            {
                _instances.Add(this);
            }
        }

        public bool IsGlobal { get; }
        public FrameworkElement Owner { get; }
        public NodeSelector NodeSelector { get; }
        public bool IsBusy
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(IsBusy));
                    field = value;
                    InvokePropertyChanged(nameof(IsBusy));
                }
            }
        }

        private readonly TranslateTransform _translation;
        private CancellationTokenSource? _currentSelectionTokenSource;

        #region Управление

        public async Task<NodeSelectorItemInfo?> Select(NodeSelectionMode mode, DialogNodeConnectionType type)
        {
            return await Select(mode, type, new Point());
        }
        public async Task<NodeSelectorItemInfo?> Select(NodeSelectionMode mode, DialogNodeConnectionType type, MouseEventArgs mouse)
        {
            return await Select(mode, type, mouse.GetPosition(Owner));
        }
        public async Task<NodeSelectorItemInfo?> Select(NodeSelectionMode mode, DialogNodeConnectionType type, Point position)
        {
            if (_currentSelectionTokenSource != null)
            {
                ClearCancellationSource(ref _currentSelectionTokenSource);
            }
            else if (IsBusy && _currentSelectionTokenSource == null)
            {
                throw new InvalidOperationException("Невозможно отменить предыдущий выбор");
            }

            IsBusy = true;
            CancellationTokenSource cancellationSource = new();
            NodeSelectorItemInfo? result = null;
            _currentSelectionTokenSource = cancellationSource;

            void OnNodeSelected(object? sender, ItemEventArgs<NodeSelectorItemInfo> e)
            {
                result = e.Item;
            }

            NodeSelector.NodeSelected += OnNodeSelected;
            NodeSelector.Mode = mode;
            NodeSelector.PortsType = type;
            NodeSelector.SearchValue = null;
            NodeSelector.Opacity = 0;
            NodeSelector.Visibility = Visibility.Visible;
            NodeSelector.IsHitTestVisible = true;
            FocusHelper.SetIgnoreFocusSwitch(NodeSelector, true);

            await Task.Delay(10);

            _translation.X = position.X - NodeSelector.RenderSize.Width / 2;
            _translation.Y = position.Y - 20;
            NodeSelector.Opacity = 1;
            NodeSelector.FocusSearch();

            await Task.Delay(10);
            FocusHelper.SetIgnoreFocusSwitch(NodeSelector, false);

            while (NodeSelector.IsVisible && result == null && 
                   !cancellationSource.IsCancellationRequested)
            {
                await Task.Delay(10);
            }

            NodeSelector.NodeSelected -= OnNodeSelected;

            IsBusy = false;

            if (_currentSelectionTokenSource == cancellationSource &&
                !cancellationSource.IsCancellationRequested)
            {
                ClearCancellationSource(ref _currentSelectionTokenSource);
            }

            Hide();

            return result;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            NodeSelector.LostFocus -= OnNodeSelectorLostFocus;

            ClearCancellationSource(ref _currentSelectionTokenSource);

            if (IsGlobal)
            {
                _instances.Remove(this);
            }
        }

        private void Hide()
        {
            NodeSelector.Visibility = Visibility.Collapsed;
            NodeSelector.IsHitTestVisible = false;
            _translation.X = -10000;
            _translation.Y = -10000;
        }
        private void ClearCancellationSource(ref CancellationTokenSource? cancellationSource)
        {
            if (cancellationSource == null)
            {
                return;
            }

            try
            {
                cancellationSource.Cancel();
                cancellationSource.Dispose();
                cancellationSource = null;
            }
            catch (Exception error)
            {
                Debug.WriteLine(error);
            }
        }

        #endregion

        #region События

        private async void OnNodeSelectorLostFocus(object sender, RoutedEventArgs e)
        {
            var nodeSelector = await Owner.Fetch<FrameworkElement, NodeSelector>(Mouse.GetPosition(Owner));

            if (nodeSelector == null)
            {
                Hide();
            }
        }

        #endregion

        #region Статика

        private static readonly List<NodeSelectorController> _instances = [];

        public static async Task<DialogProjectDialogNode?> Request(ProjectDialog dialog, MouseEventArgs? mouse, Point nodePosition)
        {
            return await CreateNode(dialog, NodeSelectionMode.Default, DialogNodeConnectionType.Action, null, mouse, nodePosition);
        }
        public static async Task<DialogProjectDialogNode?> Request(ProjectDialog dialog, DialogProjectNodePortProxy port, MouseEventArgs? mouse, Point nodePosition)
        {
            var mode = port.Original is DialogProjectNodeInput ? NodeSelectionMode.Output : NodeSelectionMode.Input;
            var type = port.Original.ConnectionType;

            return await CreateNode(dialog, mode, type, port, mouse, nodePosition);
        }

        private static async Task<DialogProjectDialogNode?> CreateNode(ProjectDialog dialog, NodeSelectionMode mode, DialogNodeConnectionType type, DialogProjectNodePortProxy? port, MouseEventArgs? mouse, Point nodePosition)
        {
            var controller = GetFirstFreeController();
            Point position;

            if (mouse != null)
            {
                position = mouse.GetPosition(controller.Owner);
            }
            else
            {
                position = Mouse.GetPosition(controller.Owner);
            }

            var info = await controller.Select(mode, type, position);

            if (info == null || info.Value == null)
            {
                return null;
            }

            return info.CreateNode(dialog, nodePosition, port);
        }

        private static NodeSelectorController GetFirstFreeController()
        {
            foreach (var selector in _instances)
            {
                if (!selector.IsBusy)
                {
                    return selector;
                }
            }

            throw new InvalidOperationException("Все контроллеры селектора узлов заняты");
        }

        #endregion
    }
}

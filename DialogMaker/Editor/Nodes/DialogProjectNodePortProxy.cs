using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Lib.Controllers;
using DialogMaker.Lib.Elements;
using System.ComponentModel;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace DialogMaker.Editor
{
    public class DialogProjectNodePortProxy : Disposable
    {
        public DialogProjectNodePortProxy(DialogProjectNode node, DialogProjectNodePort port, DialogProjectNodeMetadata metadata)
        {
            Node = node;
            Original = port;
            Description = metadata.Description;

            if (!_colorBrushes.TryGetValue(port.Color, out var colorBrush))
            {
                colorBrush = new(port.Color.ToWindows());
                _colorBrushes.Add(port.Color, colorBrush);
            }

            Color = colorBrush;

            UpdateInverted();

            port.Disposed += OnOriginalDisposed;
            port.PropertyChanged += OnPortPropertyChanged;
            node.PropertyChanged += OnNodePropertyChanged;
        }

        public DialogProjectNode Node { get; }
        public DialogProjectNodePort Original { get; }
        public string Name => Original.Name;
        public string Description { get; }
        public SolidColorBrush Color { get; }
        public bool Inverted
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Inverted));
                    field = value;
                    OnPropertyChanged(nameof(Inverted));
                }
            }
        }
        public bool IsActive => ConnectionsCount > 0;
        public int ConnectionsCount => Original.ConnectionsCount;
        public DiagramNodePort View
        {
            get
            {
                if (IsDisposed)
                {
                    throw new InvalidOperationException("Элемент очищен, взаимодействие невозможно");
                }
                if (_view == null)
                {
                    _view = Node.Project.NodesViewFabric.GetPort();
                    _viewController ??= new(this);
                }

                return _view;
            }
        }

        private DiagramNodePort? _view;
        private DiagramNodePortController? _viewController;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            _viewController?.Dispose();
            _viewController = null;

            if (_view != null)
            {
                _view.RemoveFromParent();
                Node.Project.NodesViewFabric.Free(_view);
                _view = null;
            }

            Original.PropertyChanged -= OnPortPropertyChanged;
            Node.PropertyChanged -= OnNodePropertyChanged;
        }

        private void UpdateInverted()
        {
            Inverted = Original is DialogProjectNodeInput ^ Node.Inverted;
        }

        #endregion

        #region События

        private void OnPortPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ConnectionsCount))
            {
                OnPropertyChanged(nameof(IsActive));
                OnPropertyChanged(e);
            }
            else if (e.PropertyName == nameof(Name))
            {
                OnPropertyChanged(e);
            }
        }
        private void OnNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Inverted))
            {
                UpdateInverted();
            }
        }
        private void OnOriginalDisposed(object? sender, EventArgs e)
        {
            Dispose();
        }

        #endregion

        #region Статика

        private static readonly Dictionary<Color, SolidColorBrush> _colorBrushes = [];

        public static EditableCollection<DialogProjectNodePortProxy> GetOutputs(DialogProjectNode node)
        {
            return GetPorts(node, node.Original.GetOutputs());
        }
        public static EditableCollection<DialogProjectNodePortProxy> GetInputs(DialogProjectNode node)
        {
            return GetPorts(node, node.Original.GetInputs());
        }

        private static EditableCollection<DialogProjectNodePortProxy> GetPorts<T>(DialogProjectNode node, ReferenceReadOnlyDictionary<T, DialogProjectNodeMetadata> ports)
            where T : DialogProjectNodePort
        {
            EditableCollection<DialogProjectNodePortProxy> result = [];

            foreach (var port in ports)
            {
                result.Add(new(node, port.Key, port.Value));
            }

            return result;
        }

        #endregion
    }
}

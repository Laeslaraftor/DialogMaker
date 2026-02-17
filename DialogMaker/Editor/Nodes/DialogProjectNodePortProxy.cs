using Acly;
using DialogMaker.Core;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Lib.Controllers;
using DialogMaker.Lib.Elements;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace DialogMaker.Editor
{
    public class DialogProjectNodePortProxy : ObservableObject, IDisposable
    {
        public DialogProjectNodePortProxy(DialogProjectNode node, DialogProjectNodePort port, DialogProjectNodeMetadata metadata)
        {
            Node = node;
            Original = port;
            Name = metadata.Name;
            Description = metadata.Description;

            if (!_colorBrushes.TryGetValue(port.Color, out var colorBrush))
            {
                colorBrush = new(port.Color.ToWindows());
                _colorBrushes.Add(port.Color, colorBrush);
            }

            Color = colorBrush;

            port.PropertyChanged += OnPortPropertyChanged;
        }
        ~DialogProjectNodePortProxy()
        {
            Dispose();
        }

        public bool IsDisposed { get; private set; }
        public DialogProjectNode Node { get; }
        public DialogProjectNodePort Original { get; }
        public string Name { get; }
        public string Description { get; }
        public SolidColorBrush Color { get; }
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
        private bool _isDisposing;

        #region Управление

        public void Dispose()
        {
            if (IsDisposed || _isDisposing)
            {
                return;
            }

            _isDisposing = true;
            _viewController?.Dispose();
            _viewController = null;

            if (_view != null)
            {
                _view.RemoveFromParent();
                Node.Project.NodesViewFabric.Free(_view);
                _view = null;
            }

            Original.PropertyChanged -= OnPortPropertyChanged;

            IsDisposed = true;

            GC.SuppressFinalize(this);
        }

        #endregion

        #region События

        private void OnPortPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ConnectionsCount))
            {
                InvokePropertyChanged(nameof(IsActive));
                InvokePropertyChanged(e.PropertyName);
            }
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

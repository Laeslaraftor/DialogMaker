using DialogMaker.Core;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Lib.Controllers;
using DialogMaker.Lib.Elements;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;
using System.Xml.Linq;
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
        public bool IsActive => Original.ConnectionsCount > 0;
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

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            _viewController?.Dispose();
            _viewController = null;

            if (_view != null)
            {
                Node.Project.NodesViewFabric.Free(_view);
                _view = null;
            }

            Original.PropertyChanged -= OnPortPropertyChanged;

            GC.SuppressFinalize(this);
        }

        #endregion

        #region События

        private void OnPortPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ConnectionsCount")
            {
                InvokePropertyChanged(nameof(IsActive));
            }
        }

        #endregion

        #region Статика

        private static readonly Dictionary<Color, SolidColorBrush> _colorBrushes = [];

        public static List<DialogProjectNodePortProxy> GetOutputs(DialogProjectNode node)
        {
            return GetPorts(node, node.Original.GetOutputs());
        }
        public static List<DialogProjectNodePortProxy> GetInputs(DialogProjectNode node)
        {
            return GetPorts(node, node.Original.GetInputs());
        }

        private static List<DialogProjectNodePortProxy> GetPorts<T>(DialogProjectNode node, ReadOnlyDictionary<T, DialogProjectNodeMetadata> ports)
            where T : DialogProjectNodePort
        {
            List<DialogProjectNodePortProxy> result = [];

            foreach (var port in ports)
            {
                result.Add(new(node, port.Key, port.Value));
            }

            return result;
        }

        #endregion
    }
}

using DialogMaker.Core;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Lib.Controllers;
using DialogMaker.Lib.Elements;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace DialogMaker.Editor
{
    public class DialogProjectNodePortProxy : ObservableObject, IDisposable
    {
        public DialogProjectNodePortProxy(DialogProjectNode node, DialogProjectNodePort port, string name, string description)
        {
            Node = node;
            Original = port;
            Name = name;
            Description = description;

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
            return GetPorts<NodeOutputAttribute>(node);
        }
        public static List<DialogProjectNodePortProxy> GetInputs(DialogProjectNode node)
        {
            return GetPorts<NodeInputAttribute>(node);
        }

        private static List<DialogProjectNodePortProxy> GetPorts<T>(DialogProjectNode proxy)
            where T : Attribute
        {
            List<DialogProjectNodePortProxy> result = [];
            DialogProjectDialogNode node = proxy.Original;

            foreach (var property in node.GetType().GetProperties())
            {
                var attribute = property.GetCustomAttribute<T>();

                if (attribute != null)
                {
                    if (property.GetValue(node) is not DialogProjectNodePort port)
                    {
                        continue;
                    }

                    var name = attribute.GetType().GetProperty("Name")?.GetValue(attribute) as string;
                    string description = property.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
                    result.Add(new(proxy, port, name ?? string.Empty, description));
                }
            }

            return result;
        }

        #endregion
    }
}

using DialogMaker.Core;
using DialogMaker.Core.Editor.Nodes;
using System.Reflection;
using System.Windows.Media;
using Color = System.Drawing.Color;
using System.ComponentModel;

namespace DialogMaker.Editor
{
    public class DialogProjectNodePortProxy : ObservableObject, IDisposable
    {
        public DialogProjectNodePortProxy(DialogProjectNodePort port, string name)
        {
            Original = port;
            Name = name;
            Description = port.GetType().GetDescription();

            if (!_colorBrushes.TryGetValue(port.Color, out var colorBrush))
            {
                colorBrush = new SolidColorBrush(port.Color.ToWindows());
            }

            Color = colorBrush;

            port.PropertyChanged += OnPortPropertyChanged;
        }
        ~DialogProjectNodePortProxy()
        {
            Dispose();
        }

        public DialogProjectNodePort Original { get; }
        public string Name { get; }
        public string Description { get; }
        public Brush Color { get; }
        public bool IsActive => Original.ConnectionsCount > 0;

        #region Управление

        public void Dispose()
        {
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

        private static readonly Dictionary<Color, Brush> _colorBrushes = [];

        public static List<DialogProjectNodePortProxy> GetOutputs(DialogProjectDialogNode node)
        {
            return GetPorts<NodeOutputAttribute>(node);
        }
        public static List<DialogProjectNodePortProxy> GetInputs(DialogProjectDialogNode node)
        {
            return GetPorts<NodeInputAttribute>(node);
        }

        private static List<DialogProjectNodePortProxy> GetPorts<T>(DialogProjectDialogNode node)
            where T : Attribute
        {
            List<DialogProjectNodePortProxy> result = [];

            foreach (var property in node.GetType().GetProperties())
            {
                var attribute = property.GetCustomAttribute<T>();

                if (attribute != null)
                {
                    var port = property.GetValue(node) as DialogProjectNodePort;

                    if (port == null)
                    {
                        continue;
                    }

                    var name = attribute.GetType().GetProperty("Name")?.GetValue(attribute) as string;
                    result.Add(new(port, name ?? string.Empty));
                }
            }

            return result;
        }

        #endregion
    }
}

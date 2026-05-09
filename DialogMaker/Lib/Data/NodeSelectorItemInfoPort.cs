using DialogMaker.Core;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Editor;
using System.Reflection;

namespace DialogMaker.Lib.Data
{
    public readonly struct NodeSelectorItemInfoPort(Func<DialogProjectDialogNode, DialogProjectNodePort?> portGetter, string name, DialogNodePortDirection direction, DialogNodeConnectionType connectionType)
    {
        public string Name { get; } = name;
        public DialogNodePortDirection Direction { get; } = direction;
        public DialogNodeConnectionType ConnectionType { get; } = connectionType;

        private readonly Func<DialogProjectDialogNode, DialogProjectNodePort?> _portGetter = portGetter;

        #region Управление

        public void Connect(DialogProjectDialogNode node, DialogProjectNodePortProxy port)
        {
            var nodePort = _portGetter(node);

            if (nodePort == null)
            {
                throw new InvalidOperationException($"Невозможно автоматически подключить узел, так как не удалось получить необходимый порт созданного узла: {Name}");

            }

            nodePort.Connect(port.Original);
        }

        public static implicit operator NodeSelectorItemInfoPort(KeyValuePair<PropertyInfo, DialogProjectNodeMetadata> pair)
        {
            return Create(pair);
        }

        #endregion

        #region Статика

        public static NodeSelectorItemInfoPort Create(PropertyInfo propertyInfo, DialogProjectNodeMetadata metadata)
        {
            var direction = GetDirection(propertyInfo);
            var connectionType = GetConnectionType(propertyInfo.PropertyType);
            return new(n => propertyInfo.GetValue(n) as DialogProjectNodePort, metadata.Name, direction, connectionType);
        }
        public static NodeSelectorItemInfoPort Create(KeyValuePair<PropertyInfo, DialogProjectNodeMetadata> pair)
        {
            return Create(pair.Key, pair.Value);
        }
        public static NodeSelectorItemInfoPort Create(DialogNodePortDirection direction, DialogNodeConnectionType connectionType, string name)
        {
            return new(n =>
            {
                return FindPortByName(direction, n, name);
            }, name, direction, connectionType);
        }

        private static DialogProjectNodePort? FindPortByName(DialogNodePortDirection direction, DialogProjectDialogNode node, string name)
        {
            return node.GetPorts().FirstOrDefault(p => p.Direction == direction && p.Name == name);
        }
        private static DialogNodePortDirection GetDirection(PropertyInfo portProperty)
        {
            if (typeof(DialogProjectNodeInput).IsAssignableFrom(portProperty.PropertyType))
            {
                return DialogNodePortDirection.Input;
            }

            return DialogNodePortDirection.Output;
        }
        private static DialogNodeConnectionType GetConnectionType(Type portType)
        {
            if (portType != typeof(DialogProjectNodeInputAction) &&
                portType != typeof(DialogProjectNodeOutputAction))
            {
                return DialogNodeConnectionType.Data;
            }

            return DialogNodeConnectionType.Action;
        }

        #endregion
    }
}

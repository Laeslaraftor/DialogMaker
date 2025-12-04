using Acly;
using System;
using System.Collections;
using System.IO;

namespace DialogMaker.Core.Editor.Nodes
{
    public abstract class DialogProjectNodePort : ObservableObject, ISavable, IDisposable
    {
        protected DialogProjectNodePort(INode node, string portName, DialogNodePortType dataType)
            : this(node, portName, dataType.ToConnectionType(), dataType)
        {
        }
        protected DialogProjectNodePort(INode node, string portName, DialogNodeConnectionType connectionType, DialogNodePortType dataType)
        {
            Name = portName;
            Node = node;
            ConnectionType = connectionType;
            DataType = dataType;
            ConnectionsList.ItemChanged += OnConnectionsListItemChanged;
        }
        ~DialogProjectNodePort()
        {
            Dispose(false);
        }

        public string Name { get; }
        public INode Node { get; }
        public DialogNodeConnectionType ConnectionType { get; }
        public DialogNodePortType DataType { get; }

        protected abstract IEditableList ConnectionsList { get; }

        #region Управление

        public bool IsConnected(DialogProjectNodePort? port)
        {
            if (port != null && ConnectionsList is IEnumerable enumerable)
            {
                foreach (var connection in enumerable)
                {
                    if (connection is DialogProjectNodePort connectedPort &&
                        connectedPort == port)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public bool CanConnect(DialogProjectNodePort? port)
        {
            return port != null &&
                   port != this &&
                   ConnectionType == port.ConnectionType &&
                   Node.DataConverter.CanConvert(DataType, port.DataType) &&
                   Validate(port);
        }
        public void Connect(DialogProjectNodePort? port)
        {
            if (port == null || IsConnected(port))
            {
                return;
            }

            ConnectionsList.AddNew(port);
        }
        public void Disconnect(DialogProjectNodePort? port)
        {
            if (port == null || !IsConnected(port))
            {
                return;
            }

            ConnectionsList.Remove(port);
        }

        public DialogProjectNodePortSavedState Save()
        {
            var result = CreateSavedState();

            if (ConnectionsList is IEnumerable enumerable)
            {
                foreach (var connection in enumerable)
                {
                    if (connection is DialogProjectNodePort port)
                    {
                        result.Connections.TryAdd(port.Node.Id.ToString(), port.Name);
                    }
                }
            }

            return result;
        }
        ISavedState ISavable.Save() => Save();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public override string ToString()
        {
            string nextNodeName = "?";
            string currentConnection = $"[{Node.NodeType}] {Name}";

            if (ConnectionsList is IEnumerable enumerable)
            {
                foreach (var connection in enumerable)
                {
                    if (connection is DialogProjectNodePort port)
                    {
                        nextNodeName = $"[{port.Node.NodeType}] {port.Name}";
                        break;
                    } 
                }
            }
            if (this is DialogProjectNodeInput)
            {
                return $"{nextNodeName} -> {currentConnection}";
            }

            return $"{currentConnection} -> {nextNodeName}";
        }

        protected virtual void Dispose(bool isDisposing)
        {
            ConnectionsList.ItemChanged -= OnConnectionsListItemChanged;
        }
        protected virtual bool Validate(DialogProjectNodePort? port)
        {
            return true;
        }
        protected virtual DialogProjectNodePortSavedState CreateSavedState()
        {
            return new();
        }

        #endregion

        #region События

        protected virtual void OnConnectionChanged(CollectionItemAction action, DialogProjectNodePort port)
        {
            if (!CanConnect(port))
            {
                throw new InvalidDataException($"Невозможно установить связь для {port}");
            }
        }

        private void OnConnectionsListItemChanged(object sender, CollectionItemEventArgs e)
        {
            if (e.Item is DialogProjectNodePort port)
            {
                OnConnectionChanged(e.Action, port);
            }
        }

        #endregion
    }
}

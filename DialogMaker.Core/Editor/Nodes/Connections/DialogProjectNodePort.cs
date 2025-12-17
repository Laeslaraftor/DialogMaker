using Acly;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;

namespace DialogMaker.Core.Editor.Nodes
{
    public abstract class DialogProjectNodePort : Disposable, IEnumerable<DialogProjectNodePort>, ISavable
    {
        protected DialogProjectNodePort(INode node, int portId, DialogNodePortType dataType)
            : this(node, portId, dataType.ToConnectionType(), dataType)
        {
        }
        protected DialogProjectNodePort(INode node, int portId, DialogNodeConnectionType connectionType, DialogNodePortType dataType)
        {
            Id = portId;
            Node = node;
            ConnectionType = connectionType;
            DataType = dataType;

            ConnectionsList.ItemChanged += OnConnectionsListItemChanged;
            node.PropertyChanged += OnNodePropertyChanged;
        }

        public int Id { get; }
        public INode Node { get; }
        public DialogNodeConnectionType ConnectionType { get; }
        public DialogNodePortType DataType { get; }
        public virtual Color Color { get; } = Color.Gray;
        public abstract int ConnectionsCount { get; }

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
                   Node != port.Node &&
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
            if (!CanConnect(port))
            {
                throw new InvalidDataException($"Невозможно установить связь для {port}");
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
        public void ClearConnections()
        {
            if (ConnectionsList is IList list)
            {
                list.Clear();
            }
        }

        public DialogProjectNodePortSavedState Save()
        {
            var result = CreateSavedState();

            if (ConnectionsList is IEnumerable enumerable)
            {
                foreach (var connection in enumerable)
                {
                    if (connection is not DialogProjectNodePort port)
                    {
                        continue;
                    }
                    if (!result.Connections.TryGetValue(port.Node.Id, out var connectedPorts))
                    {
                        connectedPorts = [];
                        result.Connections.Add(port.Node.Id, connectedPorts);
                    }

                    connectedPorts.Add(port.Id);
                }
            }

            return result;
        }
        ISavedState ISavable.Save() => Save();
        internal void Restore(DialogProjectNodePortSavedState savedState)
        {
            RestoreState(savedState);
        }

        public override string ToString()
        {
            string nextNodeName = "?";
            string currentConnection = $"[{Node.NodeType}] {Id}";

            if (ConnectionsList is IEnumerable enumerable)
            {
                foreach (var connection in enumerable)
                {
                    if (connection is DialogProjectNodePort port)
                    {
                        nextNodeName = $"[{port.Node.NodeType}] {port.Id}";
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

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            ClearConnections();

            ConnectionsList.ItemChanged -= OnConnectionsListItemChanged;
            Node.PropertyChanged -= OnNodePropertyChanged;
        }
        protected virtual bool Validate(DialogProjectNodePort? port)
        {
            return true;
        }
        protected virtual DialogProjectNodePortSavedState CreateSavedState()
        {
            return new();
        }
        protected virtual void RestoreState(DialogProjectNodePortSavedState savedState)
        {
        }

        #endregion

        #region Перечисление

        public abstract IEnumerator<DialogProjectNodePort> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region События

        protected virtual void OnConnectionChanged(CollectionItemAction action, DialogProjectNodePort port)
        {
            if (!CanConnect(port))
            {
                throw new InvalidDataException($"Невозможно установить связь для {port}");
            }
            if (action == CollectionItemAction.Add)
            {
                port.Connect(this);
            }
            else if (action == CollectionItemAction.Remove)
            {
                port.Disconnect(this);
            }

            InvokePropertyChanged(nameof(ConnectionsCount));
        }

        private void OnConnectionsListItemChanged(object sender, CollectionItemEventArgs e)
        {
            if (e.Action != CollectionItemAction.Move &&
                e.Item is DialogProjectNodePort port)
            {
                OnConnectionChanged(e.Action, port);
            }
        }
        private void OnNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDisposed" &&
                Node.IsDisposed && !IsDisposed)
            {
                Dispose();
            }
        }

        #endregion
    }
}

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeOutput : DialogProjectNodePort
    {
        public DialogProjectNodeOutput(INode node, int portId, DialogNodePortType dataType)
            : base(node, portId, dataType)
        {
        }
        public DialogProjectNodeOutput(INode node, int portId, DialogNodeConnectionType connectionType, DialogNodePortType dataType)
            : base(node, portId, connectionType, dataType)
        {
        }

        public EditableCollection<DialogProjectNodeInput> Connections
        {
            get
            {
                _connections ??= new();
                return _connections;
            }
        }
        public override int ConnectionsCount => Connections.Count;

        protected override IEditableList ConnectionsList => Connections;

        private EditableCollection<DialogProjectNodeInput>? _connections;

        #region Управление

        protected override bool Validate(DialogProjectNodePort? port)
        {
            return port is DialogProjectNodeInput;
        }

        public override IEnumerator<DialogProjectNodePort> GetEnumerator()
        {
            return Connections.GetEnumerator();
        }

        #endregion
    }
}

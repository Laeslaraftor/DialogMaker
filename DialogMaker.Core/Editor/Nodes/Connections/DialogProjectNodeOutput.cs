using Acly;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeOutput : DialogProjectNodePort
    {
        public DialogProjectNodeOutput(INode node, string portName, DialogNodePortType dataType) 
            : base(node, portName, dataType)
        {
        }
        public DialogProjectNodeOutput(INode node, string portName, DialogNodeConnectionType connectionType, DialogNodePortType dataType) 
            : base(node, portName, connectionType, dataType)
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

        protected override IEditableList ConnectionsList => Connections;

        private EditableCollection<DialogProjectNodeInput>? _connections;

        #region Управление

        protected override bool Validate(DialogProjectNodePort? port)
        {
            return port is DialogProjectNodeInput;
        }

        #endregion
    }
}

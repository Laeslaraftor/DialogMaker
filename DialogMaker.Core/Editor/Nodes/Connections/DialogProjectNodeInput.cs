using Acly;
using System;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInput : DialogProjectNodePort, IValuePort
    {
        public DialogProjectNodeInput(INode node, string portName, DialogNodePortType dataType) 
            : base(node, portName, dataType)
        {
        }
        public DialogProjectNodeInput(INode node, string portName, DialogNodeConnectionType connectionType, DialogNodePortType dataType) 
            : base(node, portName, connectionType, dataType)
        {
        }

        public EditableCollection<DialogProjectNodeOutput> Connections
        {
            get
            {
                _connections ??= new();
                return _connections;
            }
        }
        public virtual bool CanPresetValue { get; }

        protected override IEditableList ConnectionsList => Connections;
        protected virtual bool Multiconnection { get; }
        protected object PresetValue
        {
            get => ((IValuePort)this).Value;
            set => ((IValuePort)this).Value = value;
        }

        object IValuePort.Value
        {
            get
            {
                field ??= DataType.GetDefaultValue();
                return field;
            }
            set
            {
                if (field == value)
                {
                    return;
                }
                if (value == null)
                {
                    field = DataType.GetDefaultValue();
                }

                var type = Node.DataConverter.TypeOf(value);

                if (!Node.DataConverter.CanConvert(type, DataType))
                {
                    throw new ArgumentException($"Невозможно преобразовать значение из {type} в {DataType}", nameof(value));
                }

                InvokePropertyChanging("Value");
                field = Node.DataConverter.Convert(type, value, DataType);
                InvokePropertyChanging("Value");
            }
        }


        private EditableCollection<DialogProjectNodeOutput>? _connections;

        #region Управление

        protected override bool Validate(DialogProjectNodePort? port)
        {
            if (port is not DialogProjectNodeOutput output ||
                (Multiconnection && Connections.Count > 1 && !Connections.Contains(output)))
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}

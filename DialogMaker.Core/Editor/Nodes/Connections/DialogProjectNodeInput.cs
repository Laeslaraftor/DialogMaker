using Acly;
using System;
using System.Collections.Generic;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInput : DialogProjectNodePort, IValuePort
    {
        public DialogProjectNodeInput(INode node, int portId, DialogNodePortType dataType) 
            : base(node, portId, dataType)
        {
        }
        public DialogProjectNodeInput(INode node, int portId, DialogNodeConnectionType connectionType, DialogNodePortType dataType) 
            : base(node, portId, connectionType, dataType)
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
        public override int ConnectionsCount => Connections.Count;
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

        protected override DialogProjectNodePortSavedState CreateSavedState()
        {
            DialogProjectNodePortSavedState result = new();

            if (CanPresetValue)
            {
                result.Value = PresetValue;
            }

            return result;
        }

        protected override bool Validate(DialogProjectNodePort? port)
        {
            if (port is not DialogProjectNodeOutput output ||
                (Multiconnection && Connections.Count > 1 && !Connections.Contains(output)))
            {
                return false;
            }

            return true;
        }

        public override IEnumerator<DialogProjectNodePort> GetEnumerator()
        {
            return Connections.GetEnumerator();
        }

        #endregion
    }
}

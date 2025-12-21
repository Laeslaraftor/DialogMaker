using Acly;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        [Name("Значение"), Text(AllowMultiline = true)]
        public object Value
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

                InvokePropertyChanging(nameof(Value));
                field = Node.DataConverter.Convert(type, value, DataType);
                InvokePropertyChanged(nameof(Value));
            }
        }

        protected override IEditableList ConnectionsList => Connections;
        protected virtual bool Multiconnection { get; }

        private EditableCollection<DialogProjectNodeOutput>? _connections;

        #region Управление

        protected override DialogProjectNodePortSavedState CreateSavedState()
        {
            DialogProjectNodePortSavedState result = new();

            if (CanPresetValue)
            {
                result.Value = Value;
            }

            return result;
        }
        protected override void RestoreState(DialogProjectNodePortSavedState savedState)
        {
            base.RestoreState(savedState);

            if (!CanPresetValue || savedState.Value == null)
            {
                return;
            }

            try
            {
                var restoredValue = savedState.Value;

                if (savedState.Value is JToken token)
                {
                    var defaultType = DataType.GetDefaultType();
                    restoredValue = token.ToObject(defaultType);
                    restoredValue ??= DataType.GetDefaultValue();
                }
                if (restoredValue == null)
                {
                    return;
                }

                Value = restoredValue;
            }
            catch (Exception error)
            {
                Debug.WriteLine(error);
            }
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

using Acly;
using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

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
                field = value;
                var valueType = GetValueType(value);

                if (valueType != null)
                {
                    CurrentValueType = valueType.Value;
                }

                InvokePropertyChanged(nameof(Value));
            }
        }
        public virtual Type ReflectionValueType
        {
            get
            {
                field ??= DataType.GetDefaultType();
                return field;
            }
        }
        public AllowedObjectValues CurrentValueType
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(CurrentValueType));
                    field = value;
                    InvokePropertyChanged(nameof(CurrentValueType));
                }
            }
        }
        public virtual AllowedObjectValues AllowedValues => AllowedObjectValues.AllWithoutList;
        public virtual DialogResourceType? ResourceType => null;
        public override bool Multiconnection => false;

        protected override IEditableList ConnectionsList => Connections;

        private EditableCollection<DialogProjectNodeOutput>? _connections;

        #region Управление

        protected virtual object GetValueToSave()
        {
            if (Value is IResourceItem resource)
            {
                return resource.CreateReference();
            }

            return Value;
        }

        protected override DialogProjectNodePortSavedState CreateSavedState()
        {
            DialogProjectNodePortSavedState result = new();

            if (CanPresetValue)
            {
                var currentType = GetType();
                var valueToSave = GetValueToSave();

                result.ValueType = valueToSave?.GetType();
                result.Value = valueToSave;
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
                    if (savedState.ValueType == null)
                    {
                        var defaultType = DataType.GetDefaultType();
                        restoredValue = token.ToObject(defaultType);
                        restoredValue ??= DataType.GetDefaultValue();
                    }
                    else
                    {
                        restoredValue = token.ToObject(savedState.ValueType);

                        if (restoredValue is DialogItemReference reference)
                        {
                            restoredValue = reference.GetItem(Node.Owner);
                        }
                    }
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
                (!Multiconnection && Connections.Count > 1) ||
                Connections.Contains(output))
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

        #region Статика

        private static readonly AllowedObjectValues[] _allowedValues = [.. Enum.GetValues(typeof(AllowedObjectValues)).Cast<AllowedObjectValues>()];
        private static readonly Dictionary<AllowedObjectValues, ReadOnlyCollection<TypeAttribute>> _allowedValuesInfo = [];

        public static AllowedObjectValues? GetValueType(object? value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is IResourceItem || value is DialogProjectReference)
            {
                return AllowedObjectValues.Resource;
            }
            if (_allowedValuesInfo.Count == 0)
            {
                foreach (var allowedValue in _allowedValues)
                {
                    var typeAttributes = allowedValue.GetEnumAttributes<TypeAttribute>();

                    if (typeAttributes.Count > 0)
                    {
                        _allowedValuesInfo.Add(allowedValue, new(typeAttributes));
                    }
                }
            }

            Type objType = value.GetType();

            foreach (var info in _allowedValuesInfo)
            {
                foreach (var typeInfo in info.Value)
                {
                    if (typeInfo.Type == objType)
                    {
                        return info.Key;
                    }
                }
            }

            return null;
        }

        #endregion
    }
}

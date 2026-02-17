using System;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputValue<T>(INode node, int portId, DialogNodePortType dataType)
        : DialogProjectNodeInputValue(node, portId, dataType), IValuePort<T>
    {
        public new T Value
        {
            get => ExtractValue(base.Value);
            set
            {
                value ??= (T)DataType.GetDefaultValue();
                ValidateValue(value);
                base.Value = value;
            }
        }
        public override Type ReflectionValueType
        {
            get
            {
                field ??= typeof(T);
                return field;
            }
        }

        #region Управление

        protected virtual bool ValidateValue(T? value) => true;

        protected virtual T ExtractValue(object? value)
        {
            return (T)Convert.ChangeType(base.Value, typeof(T));
        }

        #endregion
    }
    public class DialogProjectNodeInputValue(INode node, int portId, bool canPresetValue, DialogNodePortType dataType = DialogNodePortType.Object)
        : DialogProjectNodeInput(node, portId, dataType)
    {
        public DialogProjectNodeInputValue(INode node, int portId, DialogNodePortType dataType = DialogNodePortType.Object)
            : this(node, portId, true, dataType)
        {
        }

        public override bool CanPresetValue { get; } = canPresetValue;

        #region Управление

        public string GetPreview()
        {
            foreach (var connection in this)
            {
                return connection.Node.ToString();
            }

            return Value?.ToString() ?? "null";
        }

        #endregion
    }
}

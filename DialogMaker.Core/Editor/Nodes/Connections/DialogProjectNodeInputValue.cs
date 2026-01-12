using System;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputValue<T>(INode node, int portId, DialogNodePortType dataType)
        : DialogProjectNodeInputValue(node, portId, dataType), IValuePort<T>
    {
        public new T Value
        {
            get => (T)Convert.ChangeType(base.Value, typeof(T));
            set
            {
                value ??= (T)DataType.GetDefaultValue();
                base.Value = value;
            }
        }
    }
    public class DialogProjectNodeInputValue(INode node, int portId, DialogNodePortType dataType = DialogNodePortType.Object)
        : DialogProjectNodeInput(node, portId, dataType)
    {
        public override bool CanPresetValue => true;

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

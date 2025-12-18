using Newtonsoft.Json.Linq;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputValue<T>(INode node, int portId, DialogNodePortType dataType)
#pragma warning disable CS8766 // Допустимость значений NULL для ссылочных типов в типе возвращаемого значения не соответствует неявно реализованному элементу (возможно, из-за атрибутов допустимости значений NULL).
        : DialogProjectNodeInputValue(node, portId, dataType), IValuePort<T>
#pragma warning restore CS8766 // Допустимость значений NULL для ссылочных типов в типе возвращаемого значения не соответствует неявно реализованному элементу (возможно, из-за атрибутов допустимости значений NULL).
    {
        public new T? UserValue
        {
#nullable disable
            get => ((IValuePort<T>)this).Value;
            set => ((IValuePort<T>)this).Value = value;
#nullable enable
        }

        T? IValuePort<T>.Value
        {
#nullable disable
            get
            {
                if (PresetValue is T typedValue)
                {
                    return typedValue;
                }

                return default;
            }
            set => PresetValue = value;
#nullable enable
        }
    }
    public class DialogProjectNodeInputValue(INode node, int portId, DialogNodePortType dataType = DialogNodePortType.Object)
        : DialogProjectNodeInput(node, portId, dataType)
    {
        public object? UserValue
        {
#nullable disable
            get => PresetValue;
            set => PresetValue = value;
#nullable enable
        }
        public override bool CanPresetValue => true;
    }
}

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputValue<T>(INode node, int portId, DialogNodePortType dataType)
        : DialogProjectNodeInput(node, portId, dataType), IValuePort<T>
    {
        public T Value
        {
            get => (T)PresetValue;
#pragma warning disable CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.
            set => PresetValue = value;
#pragma warning restore CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.
        }
        public override bool CanPresetValue => true;
    }
}

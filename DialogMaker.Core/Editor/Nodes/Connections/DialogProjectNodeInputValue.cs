namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputValue<T>(INode node, string portName, DialogNodePortType dataType)
        : DialogProjectNodeInput(node, portName, dataType), IValuePort<T>
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

namespace DialogMaker.Core.Editor.Nodes
{
    public interface IPortDataConverter
    {
        public DialogNodePortType TypeOf(object? instance);
        public bool CanConvert(DialogNodePortType from, DialogNodePortType to);
        public object? Convert(DialogNodePortType valueType, object? value, DialogNodePortType convertType);
    }
}

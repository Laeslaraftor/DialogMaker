namespace DialogMaker.Core
{
    public sealed class PortTypeAttribute(DialogNodePortType type) : Attribute
    {
        public DialogNodePortType PortType { get; } = type;
    }
}

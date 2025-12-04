namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputFloat(INode node, string portName)
        : DialogProjectNodeInputValue<float>(node, portName, DialogNodePortType.Float)
    {
    }
}

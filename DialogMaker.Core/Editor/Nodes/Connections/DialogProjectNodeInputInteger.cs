namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputInteger(INode node, string portName)
        : DialogProjectNodeInputValue<int>(node, portName, DialogNodePortType.Integer)
    {
    }
}

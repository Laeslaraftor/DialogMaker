namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputBool(INode node, string portName)
        : DialogProjectNodeInputValue<bool>(node, portName, DialogNodePortType.Bool)
    {
    }
}

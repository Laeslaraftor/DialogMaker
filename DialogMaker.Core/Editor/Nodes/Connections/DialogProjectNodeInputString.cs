namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputString(INode node, string portName)
        : DialogProjectNodeInputValue<string>(node, portName, DialogNodePortType.String)
    {
    }
}

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputAction(INode node, string portName) 
        : DialogProjectNodeInput(node, portName, DialogNodePortType.Action)
    {
    }
}

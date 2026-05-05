namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeOutputAction(INode node, int portId)
        : DialogProjectNodeOutput(node, portId, DialogNodePortType.Action)
    {
    }
}

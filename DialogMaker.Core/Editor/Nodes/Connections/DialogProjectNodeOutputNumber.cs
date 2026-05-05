namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeOutputNumber(INode node, int portId)
        : DialogProjectNodeOutput(node, portId, DialogNodePortType.Number)
    {
    }
}

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeOutputString(INode node, int portId)
        : DialogProjectNodeOutput(node, portId, DialogNodePortType.String)
    {
    }
}

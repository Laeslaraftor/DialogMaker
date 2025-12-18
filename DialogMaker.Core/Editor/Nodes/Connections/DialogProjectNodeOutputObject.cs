namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeOutputObject(INode node, int portId) 
        : DialogProjectNodeOutput(node, portId, DialogNodePortType.Object)
    {
    }
}

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeOutputBool(INode node, int portId) 
        : DialogProjectNodeOutput(node, portId, DialogNodePortType.Bool)
    {
    }
}

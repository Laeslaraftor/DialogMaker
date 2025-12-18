namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputFloat(INode node, int portId)
        : DialogProjectNodeInputValue<float>(node, portId, DialogNodePortType.Float)
    {
    }
}

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputInteger(INode node, int portId)
        : DialogProjectNodeInputValue<int>(node, portId, DialogNodePortType.Integer)
    {
    }
}

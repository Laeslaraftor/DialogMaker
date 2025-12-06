namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputString(INode node, int portId)
        : DialogProjectNodeInputValue<string>(node, portId, DialogNodePortType.String)
    {
    }
}

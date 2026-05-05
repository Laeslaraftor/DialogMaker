namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputAction(INode node, int portId)
        : DialogProjectNodeInput(node, portId, DialogNodePortType.Action)
    {
        public override bool Multiconnection => true;
    }
}

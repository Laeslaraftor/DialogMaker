namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputAction(INode node, int portId) 
        : DialogProjectNodeInput(node, portId, DialogNodePortType.Action)
    {
        protected override bool Multiconnection => true;
    }
}

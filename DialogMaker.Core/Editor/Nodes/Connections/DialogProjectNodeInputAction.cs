using System.Drawing;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputAction(INode node, int portId) 
        : DialogProjectNodeInput(node, portId, DialogNodePortType.Action)
    {
        public override Color Color => Color.FromArgb(43, 106, 255);

        protected override bool Multiconnection => true;
    }
}

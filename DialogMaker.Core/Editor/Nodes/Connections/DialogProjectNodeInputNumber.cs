namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputNumber(INode node, int portId)
        : DialogProjectNodeInputValue<float>(node, portId, DialogNodePortType.Number)
    {
        public override AllowedObjectValues AllowedValues => AllowedObjectValues.Number;
    }
}

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputBool(INode node, int portId)
        : DialogProjectNodeInputValue<bool>(node, portId, DialogNodePortType.Bool)
    {
        public override AllowedObjectValues AllowedValues => AllowedObjectValues.Bool;
    }
}

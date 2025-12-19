namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNumberNode : DialogProjectDialogNode
    {
        public DialogProjectNumberNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectNumberNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Number;
        [NodeInput("Значение")]
        public DialogProjectNodeInputNumber Input
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeOutput("Значение")]
        public DialogProjectNodeOutputNumber Output
        {
            get
            {
                field ??= new(this, 1);
                return field;
            }
        }
    }
}

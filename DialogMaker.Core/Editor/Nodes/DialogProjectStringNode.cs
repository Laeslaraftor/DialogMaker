namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectStringNode : DialogProjectDialogNode
    {
        public DialogProjectStringNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectStringNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.String;
        [NodeInput("Значение")]
        public DialogProjectNodeInputString Input
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeOutput("Значение")]
        public DialogProjectNodeOutputString Output
        {
            get
            {
                field ??= new(this, 1);
                return field;
            }
        }
    }
}

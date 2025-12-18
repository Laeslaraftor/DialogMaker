namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectDoIfNode : DialogProjectDialogNode
    {
        public DialogProjectDoIfNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectDoIfNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.DoIf;
        [NodeInput("Правда/ложь")]
        public DialogProjectNodeInputValue CompareValue
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeOutput("Правда")]
        public DialogProjectNodeOutputAction TrueOutput
        {
            get
            {
                field ??= new(this, 1);
                return field;
            }
        }
        [NodeOutput("Ложь")]
        public DialogProjectNodeOutputAction FalseOutput
        {
            get
            {
                field ??= new(this, 2);
                return field;
            }
        }
    }
}

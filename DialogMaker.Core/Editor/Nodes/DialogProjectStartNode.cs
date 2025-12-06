namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectStartNode : DialogProjectDialogNode
    {
        public DialogProjectStartNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectStartNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) 
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Start;
        [NodeInput("Точка входа")]
        public DialogProjectNodeOutput Begin
        {
            get
            {
                field ??= new(this, 0, DialogNodePortType.Action);
                return field;
            }
        }

        protected override DialogProjectDialogNodeSavedState CreateSavedState()
        {
            return new();
        }
    }
}

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectEndNode : DialogProjectDialogNode
    {
        public DialogProjectEndNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectEndNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Start;
        public DialogProjectNodeInputAction End
        {
            get
            {
                field ??= new(this, nameof(End));
                return field;
            }
        }

        protected override DialogProjectDialogNodeSavedState CreateSavedState()
        {
            return new();
        }
    }
}

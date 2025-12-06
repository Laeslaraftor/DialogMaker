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
        [NodeInput("Действие")]
        public DialogProjectNodeInputAction End
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }

        protected override DialogProjectDialogNodeSavedState CreateSavedState()
        {
            return new();
        }
    }
}

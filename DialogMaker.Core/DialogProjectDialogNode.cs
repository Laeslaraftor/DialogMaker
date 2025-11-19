namespace DialogMaker.Core
{
    public abstract class DialogProjectDialogNode
    {
        protected DialogProjectDialogNode(DialogProjectDialog dialog)
        {
            Dialog = dialog;
        }

        public DialogProjectDialog Dialog { get; }
        public string Id { get; }
    }
}

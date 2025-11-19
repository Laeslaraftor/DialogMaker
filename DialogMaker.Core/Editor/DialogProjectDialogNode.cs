using System;

namespace DialogMaker.Core
{
    public abstract class DialogProjectDialogNode
    {
        protected DialogProjectDialogNode(DialogProjectDialog dialog) : this(Guid.NewGuid(), dialog)
        {
        }
        protected DialogProjectDialogNode(Guid id, DialogProjectDialog dialog)
        {
            Dialog = dialog;
            Id = id;
        }

        public DialogProjectDialog Dialog { get; }
        public Guid Id { get; }
    }
}

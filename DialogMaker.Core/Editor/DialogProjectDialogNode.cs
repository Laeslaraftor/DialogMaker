using System;

namespace DialogMaker.Core.Editor
{
    public abstract class DialogProjectDialogNode : ISavable
    {
        protected DialogProjectDialogNode(DialogProjectDialog dialog)
        {
            Dialog = dialog;
            Id = Guid.NewGuid();
        }
        protected DialogProjectDialogNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
        {
            Id = Guid.Parse(savedState.Id);
            Dialog = dialog;
        }

        public DialogProjectDialog Dialog { get; }
        public Guid Id { get; }
        public abstract DialogNodeType NodeType { get; }

        #region Управление

        public DialogProjectDialogNodeSavedState Save()
        {
            var savedState = CreateSavedState();
            savedState.Id = Id.ToString();
            savedState.NodeType = NodeType;

            return savedState;
        }

        ISavedState ISavable.Save() => Save();

        protected abstract DialogProjectDialogNodeSavedState CreateSavedState();

        #endregion

        #region Статика

        public static DialogProjectDialogNode Create(DialogProjectDialog dialog, DialogNodeType type)
        {
            throw new NotImplementedException();
        }
        public static DialogProjectDialogNode Restore(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

using System;
using System.Numerics;

namespace DialogMaker.Core.Editor.Nodes
{
    public abstract class DialogProjectDialogNode : ObservableObject, INode, ISavable
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
            Position = savedState.Position;
        }

        public DialogProjectDialog Dialog { get; }
        public Guid Id { get; }
        public abstract DialogNodeType NodeType { get; }
        public Vector2 Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    InvokePropertyChanged(nameof(Position));
                }
            }
        }
        public IPortDataConverter DataConverter => DialogProjectPortDataConverter.Instance;

        private Vector2 _position;

        #region Управление

        public DialogProjectDialogNodeSavedState Save()
        {
            var savedState = CreateSavedState();
            savedState.Id = Id.ToString();
            savedState.NodeType = NodeType;
            savedState.Position = Position;

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

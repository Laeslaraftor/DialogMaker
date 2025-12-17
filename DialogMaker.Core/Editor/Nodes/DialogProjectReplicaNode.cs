using System;

namespace DialogMaker.Core.Editor.Nodes
{
    [Name("Реплика")]
    public class DialogProjectReplicaNode : DialogProjectDialogNode
    {
        public DialogProjectReplicaNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectReplicaNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.SimpleReplica;
        [Name("Персонаж"), Reference(DialogResourceType.Character)]
        public DialogProjectReference<DialogProjectCharacter>? Character
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Character));
                    field = value;
                    InvokePropertyChanged(nameof(Character));
                }
            }
        }
        [Name("Текст"), Reference(DialogResourceType.String)]
        public DialogProjectReference<DialogProjectString>? Text
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Text));
                    field = value;
                    InvokePropertyChanged(nameof(Text));
                }
            }
        }
        [NodeInput("Вход")]
        public DialogProjectNodeInputAction Input
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeOutput("Выход")]
        public DialogProjectNodeOutput Output
        {
            get
            {
                field ??= new(this, 1, DialogNodePortType.Action);
                return field;
            }
        }

        #region Управление

        protected override DialogProjectDialogNodeSavedState CreateSavedState()
        {
            var savedState = base.CreateSavedState();
            savedState.Properties.TryAdd(nameof(Character), Character?.Save());
            savedState.Properties.TryAdd(nameof(Text), Text?.Save());

            return savedState;
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);

            Character = savedState.RestoreReference<DialogProjectCharacter>(Project, nameof(Character));
            Text = savedState.RestoreReference<DialogProjectString>(Project, nameof(Text));
        }

        #endregion
    }
}

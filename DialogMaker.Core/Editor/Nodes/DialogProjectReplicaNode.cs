using System;

namespace DialogMaker.Core.Editor.Nodes
{
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
        [Reference(DialogResourceType.Character)]
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
        [Reference(DialogResourceType.String)]
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

        protected override DialogProjectDialogNodeSavedState CreateSavedState()
        {
            throw new NotImplementedException();
        }
    }
}

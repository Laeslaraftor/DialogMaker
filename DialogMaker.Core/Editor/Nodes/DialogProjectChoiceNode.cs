using Acly;
using System;

namespace DialogMaker.Core.Editor.Nodes
{
    [Name("Вариант ответа")]
    public class DialogProjectChoiceNode : DialogProjectDialogNode
    {
        public DialogProjectChoiceNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectChoiceNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Choice;
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
        public EditableCollection<DialogProjectReference<DialogProjectString>> Variants { get; } = [];
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
        [NodeOutput("Выбранный вариант")]
        public DialogProjectNodeOutput SelectedVarianIndex
        {
            get
            {
                field ??= new(this, 2, DialogNodePortType.Integer);
                return field;
            }
        }


        protected override DialogProjectDialogNodeSavedState CreateSavedState()
        {
            throw new NotImplementedException();
        }
    }
}

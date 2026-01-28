using DialogMaker.Core.Executioning;
using DialogMaker.Core.Executioning.Builders;

namespace DialogMaker.Core.Editor.Nodes
{
    public abstract class DialogProjectEmotionNode : DialogProjectDialogNode
    {
        public DialogProjectEmotionNode(DialogProjectDialog dialog) 
            : base(dialog)
        {
        }
        public DialogProjectEmotionNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) 
            : base(dialog, savedState)
        {
        }

        [NodeInput("Вход"), Sort(0)]
        public DialogProjectNodeInputAction Input
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeOutput("Выход")]
        public DialogProjectNodeOutputAction Output
        {
            get
            {
                field ??= new(this, 1);
                return field;
            }
        }
        [Name("Персонаж"), Reference(DialogResourceType.Character), Sort(0)]
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
        public override bool IsUserHandleNode => true;

        #region Управление

        public override void Compile(DialogCompilerContext context)
        {
            var character = Character?.Resolve();
            var emotion = GetEmotion(context);
            var emotionOpCode = context.Section.CreateOperation(DialogByteCode.Emotion);
            emotionOpCode.Arguments[1] = emotion;

            if (character != null)
            {
                emotionOpCode.Arguments[0] = new(character);
            }

            context.CompileOutputs(Output);
        }

        public override string ToString()
        {
            return Name;
        }

        protected abstract DialogExecutionParameter GetEmotion(DialogCompilerContext context);

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);
            savedState.Properties.TryAdd(nameof(Character), Character?.Save());
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);
            Character = savedState.RestoreReference<DialogProjectCharacter>(Project, nameof(Character));
        }

        #endregion
    }
}

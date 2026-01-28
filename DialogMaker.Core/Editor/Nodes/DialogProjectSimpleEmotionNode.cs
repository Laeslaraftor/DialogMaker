using DialogMaker.Core.Executioning;
using DialogMaker.Core.Executioning.Builders;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectSimpleEmotionNode : DialogProjectEmotionNode
    {
        public DialogProjectSimpleEmotionNode(DialogProjectDialog dialog) 
            : base(dialog)
        {
        }
        public DialogProjectSimpleEmotionNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) 
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Emotion;
        [Name("Эмоция"), Reference(DialogResourceType.Emotion)]
        public DialogProjectReference<DialogProjectEmotion>? Emotion
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Emotion));
                    field = value;
                    InvokePropertyChanged(nameof(Emotion));
                }
            }
        }

        #region Управление

        protected override DialogExecutionParameter GetEmotion(DialogCompilerContext context)
        {
            var emotion = Emotion;

            if (emotion == null)
            {
                return new();
            }

            return new(emotion.Resolve());
        }

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);
            savedState.Properties.Add(nameof(Emotion), Emotion?.Save());
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);
            Emotion = savedState.RestoreReference<DialogProjectEmotion>(Project, nameof(Emotion));
        }

        #endregion
    }
}

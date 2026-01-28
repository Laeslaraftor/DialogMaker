using DialogMaker.Core.Executioning;
using DialogMaker.Core.Executioning.Builders;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectResourceEmotionNode : DialogProjectEmotionNode
    {
        public DialogProjectResourceEmotionNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectResourceEmotionNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.ResourceEmotion;
        [NodeInput("Эмоция")]
        public DialogProjectNodeInputValue Emotion
        {
            get
            {
                field ??= new(this, 2);
                return field;
            }
        }

        #region Управление

        protected override DialogExecutionParameter GetEmotion(DialogCompilerContext context)
        {
            return context.RecursiveCompileConnections(Emotion);
        }

        #endregion
    }
}

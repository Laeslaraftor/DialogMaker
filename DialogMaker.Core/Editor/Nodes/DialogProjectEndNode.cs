using DialogMaker.Core.Executioning;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectEndNode : DialogProjectDialogNode
    {
        public DialogProjectEndNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectEndNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.End;
        [NodeInput("Действие")]
        public DialogProjectNodeInputAction End
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }

        #region Управление

        public override void Compile(DialogCompilerContext context)
        {
            context.Section.CreateOperation(DialogByteCode.End);
        }

        public override string ToString()
        {
            return string.Empty;
        }

        #endregion
    }
}

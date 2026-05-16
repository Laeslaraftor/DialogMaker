using DialogMaker.Core.Executioning;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectEntryNode : DialogProjectDialogNode
    {
        public DialogProjectEntryNode(DialogProjectDialog dialog)
            : base(dialog)
        {
        }
        public DialogProjectEntryNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Entry;
        public override bool IsCodeGenerator => true;
        public override bool IsSystem => true;
        public override bool IsUserHandleNode => true;
        [NodeOutput("Начало")]
        public DialogProjectNodeOutputAction Output
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
            context.CompileOutputs(Output, false, true);
        }

        #endregion
    }
}

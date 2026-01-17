using DialogMaker.Core.Executioning;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectMaximumNode : DialogProjectNumberComparisonNode
    {
        public DialogProjectMaximumNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectMaximumNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Maximum;

        protected override DialogByteCode ComparisonCode => DialogByteCode.Above;

        #region Управление

        public override string ToString()
        {
            return $"Максимум из {FirstValue.GetPreview()} и {SecondValue.GetPreview()}";
        }

        #endregion
    }
}

using DialogMaker.Core.Executioning;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectMinimumNode : DialogProjectNumberComparisonNode
    {
        public DialogProjectMinimumNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectMinimumNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Minimum;

        protected override DialogByteCode ComparisonCode => DialogByteCode.Less;

        #region Управление

        public override string ToString()
        {
            return $"Минимум из {FirstValue.GetPreview()} и {SecondValue.GetPreview()}";
        }

        #endregion
    }
}

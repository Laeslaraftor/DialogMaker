using DialogMaker.Core.Executioning;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectMultiplyNode : DialogProjectDialogNode
    {
        public DialogProjectMultiplyNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectMultiplyNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Multiply;
        [NodeInput("Вход")]
        public DialogProjectNodeInputAction Input
        {
            get
            {
                field ??= new(this, 3);
                return field;
            }
        }
        [NodeInput("Множимое")]
        public DialogProjectNodeInputValue FirstValue
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeInput("Множитель")]
        public DialogProjectNodeInputValue SecondValue
        {
            get
            {
                field ??= new(this, 1);
                return field;
            }
        }
        [NodeOutput("Результат")]
        public DialogProjectNodeOutputObject Output
        {
            get
            {
                field ??= new(this, 2);
                return field;
            }
        }

        #region Управление

        public override void Compile(DialogCompilerContext context)
        {
            CompileMath(context, DialogByteCode.Multiply, FirstValue, SecondValue, Output);
        }

        public override string ToString()
        {
            return $"{FirstValue.GetPreview()} * {SecondValue.GetPreview()}";
        }

        #endregion
    }
}

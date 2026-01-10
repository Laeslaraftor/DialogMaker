using DialogMaker.Core.Executioning;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectAddNode : DialogProjectDialogNode
    {
        public DialogProjectAddNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectAddNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Add;
        [NodeInput("Вход")]
        public DialogProjectNodeInputAction Input
        {
            get
            {
                field ??= new(this, 3);
                return field;
            }
        }
        [NodeInput("Значение 1")]
        public DialogProjectNodeInputValue FirstValue
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeInput("Значение 2")]
        public DialogProjectNodeInputValue SecondValue
        {
            get
            {
                field ??= new(this, 1);
                return field;
            }
        }
        [NodeOutput("Сумма")]
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
            return $"{FirstValue.GetPreview()} + {SecondValue.GetPreview()}";
        }

        #endregion
    }
}

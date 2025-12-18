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
    }
}

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectReplaceNode : DialogProjectDialogNode
    {
        public DialogProjectReplaceNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectReplaceNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Replace;
        [NodeInput("Вход")]
        public DialogProjectNodeInputAction Input
        {
            get
            {
                field ??= new(this, 3);
                return field;
            }
        }
        [NodeInput("Значение")]
        public DialogProjectNodeInputValue Value
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeInput("Заменяемое")]
        public DialogProjectNodeInputValue ReplaceValue
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

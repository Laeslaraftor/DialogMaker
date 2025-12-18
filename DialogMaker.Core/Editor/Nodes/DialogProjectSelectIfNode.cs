namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectSelectIfNode : DialogProjectDialogNode
    {
        public DialogProjectSelectIfNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectSelectIfNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.SelectIf;
        [NodeInput("Значение правды")]
        public DialogProjectNodeInputValue FirstValue
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeInput("Значение лжи")]
        public DialogProjectNodeInputValue SecondValue
        {
            get
            {
                field ??= new(this, 1);
                return field;
            }
        }
        [NodeInput("Правда/ложь")]
        public DialogProjectNodeInputValue CompareValue
        {
            get
            {
                field ??= new(this, 2);
                return field;
            }
        }
        [NodeOutput("Результат")]
        public DialogProjectNodeOutputObject Output
        {
            get
            {
                field ??= new(this, 3);
                return field;
            }
        }
    }
}

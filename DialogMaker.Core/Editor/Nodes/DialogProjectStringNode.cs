namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectStringNode : DialogProjectDialogNode
    {
        public DialogProjectStringNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectStringNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.String;
        [Name("Текст"), Text(AllowMultiline = true)]
        public string? Value
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Value));
                    field = value;
                    InvokePropertyChanged(nameof(Value));
                }
            }
        }
        [NodeOutput("Значение")]
        public DialogProjectNodeOutputString Output
        {
            get
            {
                field ??= new(this, 1);
                return field;
            }
        }

        #region Управление

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);
            savedState.Properties.Add(nameof(Value), Value);
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);
            Value = savedState.GetProperty<string>(nameof(Value));
        }

        #endregion
    }
}

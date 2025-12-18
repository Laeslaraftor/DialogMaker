namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNumberNode : DialogProjectDialogNode
    {
        public DialogProjectNumberNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectNumberNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Number;
        [Name("Значение")]
        public float Value
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
        [NodeInput("Значение")]
        public DialogProjectNodeInputFloat Input
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeOutput("Значение")]
        public DialogProjectNodeOutputFloat Output
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
            savedState.Properties.TryAdd(nameof(Value), Value);
        }

        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);
            Value = savedState.GetProperty<float>(nameof(Value));
        }

        #endregion
    }
}

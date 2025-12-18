namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectCompareNode : DialogProjectDialogNode
    {
        public DialogProjectCompareNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectCompareNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Compare;
        [Name("Тип сравнения")]
        public Comparison Comparison
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Comparison));
                    field = value;
                    InvokePropertyChanged(nameof(Comparison));
                }
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
        [NodeOutput("Результат")]
        public DialogProjectNodeOutputBool Output
        {
            get
            {
                field ??= new(this, 2);
                return field;
            }
        }

        #region Управление

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);
            savedState.Properties.TryAdd(nameof(Comparison), Comparison);
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);
            Comparison = savedState.GetProperty<Comparison>(nameof(Comparison));
        }

        #endregion
    }
}

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectVariableNode : DialogProjectDialogNode
    {
        public DialogProjectVariableNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectVariableNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Variable;
        [Name("Переменная"), Reference(DialogResourceType.Variable)]
        public DialogProjectReference<DialogProjectVariable>? Variable
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Variable));
                    field = value;
                    InvokePropertyChanged(nameof(Variable));
                }
            }
        }
        [NodeInput("Ввод")]
        public DialogProjectNodeInputValue Input
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeOutput("Вывод")]
        public DialogProjectNodeOutputObject Output
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
            savedState.Properties.TryAdd(nameof(Variable), Variable?.Save());
        }

        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);
            Variable = savedState.RestoreReference<DialogProjectVariable>(Project, nameof(Variable));
        }

        #endregion
    }
}

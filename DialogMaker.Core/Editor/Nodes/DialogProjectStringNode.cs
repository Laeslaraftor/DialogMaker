using DialogMaker.Core.Executioning;

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
                    OnPropertyChanging(nameof(Value));
                    field = value;
                    OnPropertyChanged(nameof(Value));
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
        public override bool IsCodeGenerator => false;

        #region Управление

        public override void Compile(DialogCompilerContext context)
        {
            string value = Value ?? string.Empty;
            context.Resources.CreateVariable(Output, new(value));
        }
        public override string ToString()
        {
            return Value ?? string.Empty;
        }

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

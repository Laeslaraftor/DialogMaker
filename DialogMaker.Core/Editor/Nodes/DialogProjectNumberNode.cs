using DialogMaker.Core.Executioning;

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
        [Name("Число")]
        public float Value
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
        public DialogProjectNodeOutputNumber Output
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
            context.Resources.CreateVariable(Output, new(Value));
        }

        public override string ToString()
        {
            return Value.ToString().Replace(",", ".");
        }

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);
            savedState.Properties.Add(nameof(Value), Value);
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);
            Value = savedState.GetNumberProperty(nameof(Value));
        }

        #endregion
    }
}

using DialogMaker.Core.Executioning;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectReferenceNode : DialogProjectDialogNode
    {
        public DialogProjectReferenceNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectReferenceNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Reference;
        [Name("Ресурс")]
        public DialogProjectReference? Reference
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Reference));
                    field = value;
                    OnPropertyChanged(nameof(Reference));
                }
            }
        }
        [NodeOutput("Значение")]
        public DialogProjectNodeOutputObject Output
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        public override bool IsCodeGenerator => false;

        #region Управление

        public override void Compile(DialogCompilerContext context)
        {
            if (Reference == null)
            {
                return;
            }

            var resource = Reference.Resolve();
            context.Resources.CreateVariable(Output, new(resource));
        }
        public override string ToString()
        {
            var reference = Reference;

            if (reference == null)
            {
                return "null";
            }

            return reference.Resolve().ToString();
        }

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);
            savedState.Properties.TryAdd(nameof(Reference), Reference?.Save());
        }

        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);
            Reference = savedState.RestoreReference(Project, nameof(Reference));
        }

        #endregion
    }
}

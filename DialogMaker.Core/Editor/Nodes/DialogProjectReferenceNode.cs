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
                    InvokePropertyChanging(nameof(Reference));
                    field = value;
                    InvokePropertyChanged(nameof(Reference));
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

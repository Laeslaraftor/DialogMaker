using DialogMaker.Core.Executioning;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectEndNode : DialogProjectDialogNode
    {
        public DialogProjectEndNode(DialogProjectDialog dialog) 
            : base(dialog)
        {
        }
        public DialogProjectEndNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) 
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.End;
        [Name("Только поток")]
        public bool OnlyCurrentThread
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(OnlyCurrentThread));
                    field = value;
                    InvokePropertyChanged(nameof(OnlyCurrentThread));
                }
            }
        }
        [NodeInput("Действие")]
        public DialogProjectNodeInputAction End
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
            if (OnlyCurrentThread)
            {
                context.Section.CreateOperation(DialogByteCode.EndThread);
                return;
            }

            context.Section.CreateOperation(DialogByteCode.End);
        }

        public override string ToString()
        {
            return "Принудительное завершение диалога";
        }

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);
            savedState.Properties.TryAdd(nameof(OnlyCurrentThread), OnlyCurrentThread);
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);
            OnlyCurrentThread = savedState.GetProperty<bool>(nameof(OnlyCurrentThread));
        }

        #endregion
    }
}

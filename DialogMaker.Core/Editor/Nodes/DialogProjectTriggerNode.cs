using DialogMaker.Core.Executioning;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectTriggerNode : DialogProjectDialogNode
    {
        public DialogProjectTriggerNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectTriggerNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Trigger;
        [Name("Идентификатор")]
        public string? TriggerId
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(TriggerId));
                    field = value;
                    InvokePropertyChanged(nameof(TriggerId));
                }
            }
        }
        [NodeInput("Вход")]
        public DialogProjectNodeInputAction Input
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeOutput("Выход")]
        public DialogProjectNodeOutputAction Output
        {
            get
            {
                field ??= new(this, 1);
                return field;
            }
        }
        public override bool IsUserHandleNode => true;

        #region Управление

        public override void Compile(DialogCompilerContext context)
        {
            var triggerId = TriggerId;

            if (!string.IsNullOrEmpty(triggerId))
            {
                var opcode = context.Section.CreateOperation(DialogByteCode.Trigger);
                opcode.Arguments[0] = new(triggerId);
            }

            context.CompileOutputs(Output);
        }

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);
            savedState.Properties.TryAdd(nameof(TriggerId), TriggerId);
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);
            TriggerId = savedState.GetProperty<string>(nameof(TriggerId));
        }

        #endregion
    }
}

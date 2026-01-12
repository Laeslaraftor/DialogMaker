using DialogMaker.Core.Executioning;
using MessagePack;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectEventNode : DialogProjectDialogNode
    {
        public DialogProjectEventNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectEventNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Event;
        public override bool IsSystem => true;
        [Name("Событие")]
        public DialogExecutionEvent Event
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Event));
                    field = value;
                    InvokePropertyChanged(nameof(Event));
                }
            }
        }
        [NodeOutput("Выход")]
        public DialogProjectNodeOutputAction Output
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
            context.CompileOutputs(Output);
        }

        public override string ToString()
        {
            return Event.GetEnumAttribute<NameAttribute>()?.Name ?? Event.ToString();
        }

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);
            savedState.Properties.TryAdd(nameof(Event), Event);
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);
            Event = savedState.GetProperty<DialogExecutionEvent>(nameof(Event));
        }

        #endregion
    }
}

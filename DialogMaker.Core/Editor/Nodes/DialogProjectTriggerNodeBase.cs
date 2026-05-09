using DialogMaker.Core.Editor.Nodes.Structs;
using DialogMaker.Core.Executioning;

namespace DialogMaker.Core.Editor.Nodes
{
    public abstract class DialogProjectTriggerNodeBase : DialogProjectDialogNode
    {
        public DialogProjectTriggerNodeBase(DialogProjectDialog dialog)
            : base(dialog)
        {
        }
        public DialogProjectTriggerNodeBase(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
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

        protected abstract string? TriggerName { get; }

        #region Управление

        public override void Compile(DialogCompilerContext context)
        {
            TriggerNodeCompileTimeInfo info = new(TriggerName ?? string.Empty);

            foreach (var message in InternalMessages)
            {
                throw new InvalidOperationException(message.Text);
            }

            foreach (var input in ExtraInputs.Keys)
            {
                var parameter = context.RecursiveCompileConnections(input);
                info.Inputs.Add(input.Name, parameter);
            }
            foreach (var output in ExtraOutputs.Keys)
            {
                var parameter = context.Resources.GetOrCreateVariable(output);
                info.Outputs.Add(output.Name, parameter);
            }

            var opcode = context.Section.CreateOperation(DialogByteCode.Trigger);
            opcode.Arguments[0] = new(info);

            context.CompileOutputs(Output);
        }

        public override string ToString()
        {
            var id = TriggerName;

            if (string.IsNullOrEmpty(id))
            {
                return "Пустой идентификатор";
            }

            return $"Идентификатор: {id}";
        }

        #endregion
    }
}

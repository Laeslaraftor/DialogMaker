using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning;
using DialogMaker.Core.Executioning.Builders;
using System.Diagnostics.CodeAnalysis;

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
        [NodeInput("Действие")]
        public DialogProjectNodeInputAction Action
        {
            get
            {
                field ??= new(this, 2);
                return field;
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

        public override void Compile(DialogCompilerContext context)
        {
            var variable = Variable;

            if (variable == null)
            {
                if (Input.ConnectionsCount > 0 &&
                    Output.ConnectionsCount > 0)
                {
                    var input = context.RecursiveCompileConnections(Input);
                    var output = context.Resources.GetOrCreateVariable(Output);

                    var setOpCode = context.Section.CreateOperation(DialogByteCode.Set);
                    setOpCode.Arguments[0] = output;
                    setOpCode.Arguments[1] = input;
                }

                return;
            }

            var resource = variable.Resolve();
            DialogExecutionParameter parameter = new(resource);

            if (Input.ConnectionsCount > 0)
            {
                var newValueVariable = context.RecursiveCompileConnections(Input);

                var setOpCode = context.Section.CreateOperation(DialogByteCode.Set);
                setOpCode.Arguments[0] = parameter;
                setOpCode.Arguments[1] = newValueVariable;
            }
            if (Output.ConnectionsCount > 0)
            {
                var output = context.Resources.GetOrCreateVariable(Output);

                if (output.Value?.Equals(parameter.Value) == true)
                {
                    return;
                }

                var setOpCode = context.Section.CreateOperation(DialogByteCode.Set);
                setOpCode.Arguments[0] = output;
                setOpCode.Arguments[1] = parameter;

                context.CompileOutputs(Output);
            }
        }

        public override string ToString()
        {
            var variable = Variable;

            if (variable == null)
            {
                return "Локальная переменная";
            }

            return $"{variable.Resolve()} = {Input.GetPreview()}";
        }

        public override bool TryGetResourceValue(DialogProjectNodeOutput port, [NotNullWhen(true)] out IResourceItem? resource)
        {
            if (port == Output)
            {
                resource = Variable?.Resolve();
                return resource != null;
            }

            return base.TryGetResourceValue(port, out resource);
        }

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

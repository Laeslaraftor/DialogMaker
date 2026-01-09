using DialogMaker.Core.Executioning;
using DialogMaker.Core.Executioning.Builders;
using DialogMaker.Core.Executioning.Internal;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectReplaceNode : DialogProjectDialogNode
    {
        public DialogProjectReplaceNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectReplaceNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Replace;
        [NodeInput("Вход")]
        public DialogProjectNodeInputAction Input
        {
            get
            {
                field ??= new(this, 3);
                return field;
            }
        }
        [NodeInput("Значение")]
        public DialogProjectNodeInputString Value
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeInput("Заменяемое")]
        public DialogProjectNodeInputString SearchValue
        {
            get
            {
                field ??= new(this, 4);
                return field;
            }
        }
        [NodeInput("Новое значение")]
        public DialogProjectNodeInputString NewValue
        {
            get
            {
                field ??= new(this, 1);
                return field;
            }
        }
        [NodeOutput("Результат")]
        public DialogProjectNodeOutputObject Output
        {
            get
            {
                field ??= new(this, 2);
                return field;
            }
        }

        #region Управление

        public override void Compile(DialogCompilerContext context)
        {
            var value = context.Compiler.RecursiveCompileConnections(context, Value);
            var searchValue = context.Compiler.RecursiveCompileConnections(context, SearchValue);
            var newValue = context.Compiler.RecursiveCompileConnections(context, NewValue);
            var output = context.Resources.GetOrCreateVariable(Output);
            DialogExecutionParameter tempVariable = new(string.Empty);

            var setTempValueOperation = context.Section.CreateOperation(DialogByteCode.Set);
            setTempValueOperation.Arguments[0] = tempVariable;
            setTempValueOperation.Arguments[1] = value;

            var replaceOperation = context.Section.CreateOperation(DialogByteCode.Replace);
            replaceOperation.Arguments[0] = tempVariable;
            replaceOperation.Arguments[1] = searchValue;
            replaceOperation.Arguments[2] = newValue;

            var stackToOutputOperation = context.Section.CreateOperation(DialogByteCode.StackToVariable);
            stackToOutputOperation.Arguments[0] = output;

            context.CompileOutputs(Output);
        }

        #endregion
    }
}

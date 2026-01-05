using DialogMaker.Core.Executioning;

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

            var replaceOpCode = context.Section.CreateOperation(DialogByteCode.Replace);
            replaceOpCode.Arguments[0] = value;
            replaceOpCode.Arguments[1] = searchValue;
            replaceOpCode.Arguments[2] = newValue;

            var setOpCode = context.Section.CreateOperation(DialogByteCode.Set);
            setOpCode.Arguments[0] = output;
            setOpCode.Arguments[1] = value;

            context.CompileOutputs(Output);
        }

        #endregion
    }
}

using DialogMaker.Core.Executioning;
using DialogMaker.Core.Executioning.Builders;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectSelectIfNode : DialogProjectDialogNode
    {
        public DialogProjectSelectIfNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectSelectIfNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.SelectIf;
        [NodeInput("Значение правды")]
        public DialogProjectNodeInputValue FirstValue
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeInput("Значение лжи")]
        public DialogProjectNodeInputValue SecondValue
        {
            get
            {
                field ??= new(this, 1);
                return field;
            }
        }
        [NodeInput("Правда/ложь")]
        public DialogProjectNodeInputValue CompareValue
        {
            get
            {
                field ??= new(this, 2);
                return field;
            }
        }
        [NodeOutput("Результат")]
        public DialogProjectNodeOutputObject Output
        {
            get
            {
                field ??= new(this, 3);
                return field;
            }
        }

        #region Управление

        public override void Compile(DialogCompilerContext context)
        {
            var value1 = context.Compiler.RecursiveCompileConnections(context, FirstValue);
            var value2 = context.Compiler.RecursiveCompileConnections(context, SecondValue);
            var expressionResult = context.Compiler.RecursiveCompileConnections(context, CompareValue);
            var output = context.Resources.GetOrCreateVariable(Output);

            var comparison = context.Section.CreateOperation(DialogByteCode.Equals);
            comparison.Arguments[0] = expressionResult;
            comparison.Arguments[1] = new(true);

            OperationBuilder CreateSetValue(DialogExecutionParameter value)
            {
                var opCode = context.Section.CreateOperation(DialogByteCode.Set);
                opCode.Arguments[0] = output;
                opCode.Arguments[1] = value;

                return opCode;
            }

            var gotoValue1 = context.Section.CreateOperation(DialogByteCode.GotoIfTrue);
            var gotoValue2 = context.Section.CreateOperation(DialogByteCode.Goto);
            var setValue1 = CreateSetValue(value1);
            var gotoEnd = context.Section.CreateOperation(DialogByteCode.Goto);
            var setValue2 = CreateSetValue(value2);
            var ending = context.Section.CreateOperation(DialogByteCode.Empty);

            gotoValue1.Arguments[0] = new(setValue1);
            gotoValue2.Arguments[0] = new(setValue2);
            gotoEnd.Arguments[0] = new(ending);
        }

        #endregion
    }
}

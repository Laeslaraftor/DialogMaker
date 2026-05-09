using DialogMaker.Core.Executioning;

namespace DialogMaker.Core.Editor.Nodes
{
    public abstract class DialogProjectNumberComparisonNode : DialogProjectDialogNode
    {
        public DialogProjectNumberComparisonNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectNumberComparisonNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) : base(dialog, savedState)
        {
        }

        [NodeInput("Значение 1")]
        public DialogProjectNodeInputNumber FirstValue
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeInput("Значение 2")]
        public DialogProjectNodeInputNumber SecondValue
        {
            get
            {
                field ??= new(this, 1);
                return field;
            }
        }
        [NodeOutput("Результат")]
        public DialogProjectNodeOutputNumber Output
        {
            get
            {
                field ??= new(this, 2);
                return field;
            }
        }

        protected abstract DialogByteCode ComparisonCode { get; }

        #region Управление

        public override void Compile(DialogCompilerContext context)
        {
            var output = context.Resources.GetOrCreateVariable(Output);

            if (FirstValue.ConnectionsCount == 0 &&
                SecondValue.ConnectionsCount == 0)
            {
                var value = Math.Min(FirstValue.Value, SecondValue.Value);
                var operation = context.Section.CreateOperation(DialogByteCode.Set);
                operation.Arguments[0] = output;
                operation.Arguments[1] = new(value);
                return;
            }

            var first = context.RecursiveCompileConnections(FirstValue);
            var second = context.RecursiveCompileConnections(SecondValue);

            // first < second
            var comparison = context.Section.CreateOperation(ComparisonCode);
            comparison.Arguments[0] = first;
            comparison.Arguments[1] = second;

            var skipSecond = context.Section.CreateOperation(DialogByteCode.GotoIfTrue);
            var setSecond = context.Section.CreateOperation(DialogByteCode.Set);
            setSecond.Arguments[0] = second;
            setSecond.Arguments[1] = first;

            var goToEnd = context.Section.CreateOperation(DialogByteCode.Goto);
            var setFirst = context.Section.CreateOperation(DialogByteCode.Set);
            setFirst.Arguments[0] = first;
            setFirst.Arguments[1] = second;

            var ending = context.Section.CreateOperation(DialogByteCode.Empty);
            skipSecond.Arguments[0] = new(setFirst);
            goToEnd.Arguments[0] = new(ending);
        }

        #endregion
    }
}

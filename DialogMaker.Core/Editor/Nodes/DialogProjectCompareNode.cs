using DialogMaker.Core.Executioning;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectCompareNode : DialogProjectDialogNode
    {
        public DialogProjectCompareNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectCompareNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Compare;
        [NodeInput("Вход")]
        public DialogProjectNodeInputAction Input
        {
            get
            {
                field ??= new(this, 3);
                return field;
            }
        }
        [Name("Тип сравнения")]
        public Comparison Comparison
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Comparison));
                    field = value;
                    InvokePropertyChanged(nameof(Comparison));
                }
            }
        }
        [NodeInput("Значение 1")]
        public DialogProjectNodeInputValue FirstValue
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeInput("Значение 2")]
        public DialogProjectNodeInputValue SecondValue
        {
            get
            {
                field ??= new(this, 1);
                return field;
            }
        }
        [NodeOutput("Результат")]
        public DialogProjectNodeOutputBool Output
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
            var value1 = context.Compiler.RecursiveCompileConnections(context, FirstValue);
            var value2 = context.Compiler.RecursiveCompileConnections(context, SecondValue);
            var output = context.Resources.GetOrCreateVariable(Output);

            var comparison = context.Section.CreateOperation((DialogByteCode)Comparison);
            comparison.Arguments[0] = value1;
            comparison.Arguments[1] = value2;

            var stackToVariable = context.Section.CreateOperation(DialogByteCode.StackToVariable);
            stackToVariable.Arguments[0] = output;

            context.Compiler.CompileOutputs(context, Output);
        }

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);
            savedState.Properties.TryAdd(nameof(Comparison), Comparison);
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);
            Comparison = savedState.GetProperty<Comparison>(nameof(Comparison));
        }

        #endregion
    }
}

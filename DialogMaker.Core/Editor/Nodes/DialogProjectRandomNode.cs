using DialogMaker.Core.Executioning;
using System.ComponentModel;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectRandomNode : DialogProjectDialogNode
    {
        public DialogProjectRandomNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectRandomNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.RandomNumber;

        [NodeInput("Минимум"), Description("Минимальное значение диапазона")]
        public DialogProjectNodeInputNumber Min
        {
            get
            {
                field ??= new(this, 0)
                {
                    Value = 0f
                };
                return field;
            }
        }
        [NodeInput("Максимум"), Description("Максимальное значение диапазона (включительно)")]
        public DialogProjectNodeInputNumber Max
        {
            get
            {
                field ??= new(this, 1)
                {
                    Value = 1f
                };
                return field;
            }
        }
        [NodeInput("Целое число"), Description("Должно ли быть случайное число целым")]
        public DialogProjectNodeInputBool IsInteger
        {
            get
            {
                field ??= new(this, 2)
                {
                    Value = true
                };
                return field;
            }
        }
        [NodeOutput("Результат"), Description("Вывод случайного значения из заданного диапазона")]
        public DialogProjectNodeOutputNumber Output
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
            var min = context.RecursiveCompileConnections(Min);
            var max = context.RecursiveCompileConnections(Max);
            var isInteger = context.RecursiveCompileConnections(IsInteger);
            var output = context.Resources.GetOrCreateVariable(Output);

            var operation = context.Section.CreateOperation(DialogByteCode.RandomNumber);
            operation.Arguments[0] = min;
            operation.Arguments[1] = max;
            operation.Arguments[2] = isInteger;
            operation.Arguments[3] = output;
        }

        public override string ToString()
        {
            return $"Random({Min.GetPreview()}, {Max.GetPreview()}, {IsInteger.GetPreview()})";
        }

        #endregion
    }
}

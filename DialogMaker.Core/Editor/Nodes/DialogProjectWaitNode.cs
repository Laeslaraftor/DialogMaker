using DialogMaker.Core.Executioning;
using DialogMaker.Core.Executioning.Builders;
using System.ComponentModel;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectWaitNode : DialogProjectDialogNode
    {
        public DialogProjectWaitNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectWaitNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Wait;

        [NodeInput("Вход")]
        public DialogProjectNodeInputAction Input
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeInput("Продолжительность"), Description("Продолжительность ожидания в секундах")]
        public DialogProjectNodeInputNumber Time
        {
            get
            {
                field ??= new(this, 1);
                return field;
            }
        }
        [NodeOutput("Выход")]
        public DialogProjectNodeOutputAction Output
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
            if (Output.ConnectionsCount == 0)
            {
                return;
            }

            DialogExecutionParameter time;

            if (Time.ConnectionsCount == 0)
            {
                float duration = Time.Value;

                if (0 >= duration)
                {
                    return;
                }

                time = new(duration);
            }
            else
            {
                time = context.RecursiveCompileConnections(Time);
            }

            var waitOpCode = context.Section.CreateOperation(DialogByteCode.Wait);
            waitOpCode.Arguments[0] = time;

            context.CompileOutputs(Output);
        }

        public override string ToString()
        {
            return $"Ожидание: {Time.GetPreview()} секунд";
        }

        #endregion
    }
}

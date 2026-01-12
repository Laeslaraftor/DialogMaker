using DialogMaker.Core.Executioning;
using DialogMaker.Core.Executioning.Builders;
using System.Collections.Generic;
using System.Linq;

namespace DialogMaker.Core.Editor.Nodes
{
    public abstract class DialogProjectThreadNode : DialogProjectDialogNode
    {
        public DialogProjectThreadNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectThreadNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) : base(dialog, savedState)
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

        protected abstract DialogByteCode OpCode { get; }

        #region Управление

        public override void Compile(DialogCompilerContext context)
        {
            if (Input.ConnectionsCount == 0 || Output.ConnectionsCount == 0)
            {
                return;
            }

            List<INode> inputs = [.. Input.Select(c => c.Node)];
            List<INode> outputs = [.. Output.Select(c => c.Node)];
            JoinOperationInfoBuilder builder = new(context.Compiler, inputs, outputs);

            context.RecursiveCompileConnections(Input);

            var operation = context.Section.CreateOperation(OpCode);
            operation.Arguments[0] = new(builder);

            context.CompileOutputs(Output);
        }

        #endregion
    }
}

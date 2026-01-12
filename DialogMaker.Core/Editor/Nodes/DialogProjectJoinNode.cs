using DialogMaker.Core.Executioning;
using System.Text;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectJoinNode : DialogProjectThreadNode
    {
        public DialogProjectJoinNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectJoinNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Join;

        protected override DialogByteCode OpCode => DialogByteCode.Join;

        #region Управление

        public override string ToString()
        {
            StringBuilder builder = new();
            builder.AppendLine("Ожидание:");

            foreach (var connection in Input)
            {
                builder.AppendLine(connection.ToString());
            }

            builder.AppendLine("Продолжение:");

            foreach (var connection in Output)
            {
                builder.AppendLine(connection.ToString());
            }

            return builder.ToString();
        }

        #endregion
    }
}

using DialogMaker.Core.Executioning;
using System;
using System.Text;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectIntersectNode : DialogProjectThreadNode
    {
        public DialogProjectIntersectNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectIntersectNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Intersect;

        protected override DialogByteCode OpCode => DialogByteCode.Intersect;

        #region Управление

        public override string ToString()
        {
            StringBuilder builder = new();
            builder.AppendLine("После любого входа:");

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

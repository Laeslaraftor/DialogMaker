using System;
using System.Collections.Generic;
using System.Text;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectDivideNode : DialogProjectDialogNode
    {
        public DialogProjectDivideNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectDivideNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Divide;
        [NodeInput("Делимое")]
        public DialogProjectNodeInputValue FirstValue
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeInput("Делитель")]
        public DialogProjectNodeInputValue SecondValue
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
    }
}

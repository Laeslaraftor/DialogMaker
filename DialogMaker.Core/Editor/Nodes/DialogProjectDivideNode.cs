using DialogMaker.Core.Executioning;
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
        [NodeInput("Вход")]
        public DialogProjectNodeInputAction Input
        {
            get
            {
                field ??= new(this, 3);
                return field;
            }
        }
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

        #region Управление

        public override void Compile(DialogCompilerContext context)
        {
            CompileMath(context, DialogByteCode.Divide, FirstValue, SecondValue, Output);
        }

        #endregion
    }
}

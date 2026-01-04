using DialogMaker.Core.Executioning;
using DialogMaker.Core.Executioning.Builders;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectDoIfNode : DialogProjectDialogNode
    {
        public DialogProjectDoIfNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectDoIfNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.DoIf;
        [NodeInput("Правда/ложь")]
        public DialogProjectNodeInputValue CompareValue
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeOutput("Правда")]
        public DialogProjectNodeOutputAction TrueOutput
        {
            get
            {
                field ??= new(this, 1);
                return field;
            }
        }
        [NodeOutput("Ложь")]
        public DialogProjectNodeOutputAction FalseOutput
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
            var trueSections = context.Compiler.GetConnectedSections(TrueOutput);
            var falseSections = context.Compiler.GetConnectedSections(FalseOutput);

            if (trueSections.Count == 0 && falseSections.Count == 0)
            {
                return;
            }

            OperationBuilder CreateSectionsInvoke(List<DialogSectionBuilder> sections)
            {
                if (sections.Count == 1)
                {
                    var jumpOpCode = context.Section.CreateOperation(DialogByteCode.Jump);
                    jumpOpCode.Arguments[0] = new(sections[0]);
                    return jumpOpCode;
                }

                OperationBuilder? firstBuilder = null;

                foreach (var section in sections)
                {
                    var startThread = context.Section.CreateOperation(DialogByteCode.StartThread);
                    startThread.Arguments[0] = new(section);
                    firstBuilder ??= startThread;
                }

                return firstBuilder!;
            }

            var comparisonResult = context.Compiler.RecursiveCompileConnections(context, CompareValue);
            var comparison = context.Section.CreateOperation(DialogByteCode.Equals);
            comparison.Arguments[0] = comparisonResult;

            /*
             
            cmp result, true
            gotoIfTrue 1
            goto 2

            1: 
            запуск true потоков
            goto 3
            2: 
            запуск false потоков
            3:
            empty
             
             */

            void CreateSingle(bool value, List<DialogSectionBuilder> sections, List<DialogSectionBuilder>? sections2 = null)
            {
                comparison.Arguments[1] = new(value);
                var gotoIfTrue = context.Section.CreateOperation(DialogByteCode.GotoIfTrue);
                var gotoIfFalse = context.Section.CreateOperation(DialogByteCode.Goto);
                var firstSectionOperator = CreateSectionsInvoke(sections);
                var gotoEnd = context.Section.CreateOperation(DialogByteCode.Goto);
                OperationBuilder? secondSectionOperator = null;

                if (sections2 != null)
                {
                    secondSectionOperator = CreateSectionsInvoke(sections2);
                }

                var endOperator = context.Section.CreateOperation(DialogByteCode.Empty);

                gotoIfTrue.Arguments[0] = new(firstSectionOperator);
                gotoIfFalse.Arguments[0] = secondSectionOperator != null ? new(secondSectionOperator) : new(endOperator);
                gotoEnd.Arguments[0] = new(endOperator);
            }

            if (trueSections.Count > 0 && falseSections.Count > 0)
            {
                CreateSingle(true, trueSections, falseSections);
            }

            /*
             
            cmp result, true
            gotoIfTrue 1
            goto 2

            1: 
            запуск true потоков
            2:
            empty
             
             */

            else if (trueSections.Count > 0)
            {
                CreateSingle(true, trueSections);
            }

            /*
             
            cmp result, false
            gotoIfTrue 1
            goto 2

            1: 
            запуск false потоков
            2:
            empty
             
             */

            else if (falseSections.Count > 0)
            {
                CreateSingle(false, falseSections);
            }
        }

        #endregion
    }
}

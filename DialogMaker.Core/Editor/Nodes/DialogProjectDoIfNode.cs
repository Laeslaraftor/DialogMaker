using DialogMaker.Core.Executioning;
using DialogMaker.Core.Executioning.Builders;
using System;
using System.Collections.Generic;
using System.Text;

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
            var value = context.RecursiveCompileConnections(CompareValue);

            void CreateManySections(List<DialogSectionBuilder> sections)
            {
                for (int i = 0; i < sections.Count; i++)
                {
                    var code = i + 1 >= sections.Count ? DialogByteCode.Jump : DialogByteCode.StartThread;
                    var jumpIfTrue = context.Section.CreateOperation(code);
                    jumpIfTrue.Arguments[0] = new(sections[i]);
                }
            }

            if (trueSections.Count == 0 && falseSections.Count == 0)
            {
                return;
            }
            if (trueSections.Count == 1 && falseSections.Count == 0)
            {
                context.JumpOrGotoOrSkip(TrueOutput[0].Node, value, false);
                context.Section.CreateOperation(DialogByteCode.EndThread);
            }
            else if (trueSections.Count > 1 && falseSections.Count == 0)
            {
                var checkFalse = context.Section.CreateOperation(DialogByteCode.NotEquals);
                checkFalse.Arguments[0] = value;
                checkFalse.Arguments[1] = new(true);

                var skipIfFalse = context.Section.CreateOperation(DialogByteCode.GotoIfTrue);

                CreateManySections(trueSections);

                var empty = context.Section.CreateOperation(DialogByteCode.Empty);
                skipIfFalse.Arguments[0] = new(empty);
            }
            else if (trueSections.Count == 0 && falseSections.Count == 1)
            {
                context.JumpOrGotoOrSkip(TrueOutput[0].Node, value, true);
                context.Section.CreateOperation(DialogByteCode.EndThread);
            }
            else if (trueSections.Count == 0 && falseSections.Count > 1)
            {
                var checkFalse = context.Section.CreateOperation(DialogByteCode.Equals);
                checkFalse.Arguments[0] = value;
                checkFalse.Arguments[1] = new(true);

                var skipIfFalse = context.Section.CreateOperation(DialogByteCode.JumpIfTrue);

                CreateManySections(falseSections);

                var empty = context.Section.CreateOperation(DialogByteCode.Empty);
                skipIfFalse.Arguments[0] = new(empty);
            }
            else if (trueSections.Count == 1 && falseSections.Count == 1)
            {
                var checkFalse = context.Section.CreateOperation(DialogByteCode.Equals);
                checkFalse.Arguments[0] = value;
                checkFalse.Arguments[1] = new(false);

                var gotoIfFalse = context.Section.CreateOperation(DialogByteCode.GotoIfTrue);

                context.JumpOrGoto(TrueOutput[0].Node);
                var falseAction = context.JumpOrGoto(FalseOutput[0].Node);

                gotoIfFalse.Arguments[0] = new(falseAction);
            }
            else
            {
                var checkTrue = context.Section.CreateOperation(DialogByteCode.NotEquals);
                checkTrue.Arguments[0] = value;
                checkTrue.Arguments[1] = new(true);

                var skipTrue = context.Section.CreateOperation(DialogByteCode.GotoIfTrue);

                CreateManySections(trueSections);

                var startFalseSections = context.Section.CreateOperation(DialogByteCode.Empty);
                skipTrue.Arguments[0] = new(startFalseSections);

                CreateManySections(falseSections);
            }
        }

        public override string ToString()
        {
            if (TrueOutput.ConnectionsCount == 0 && FalseOutput.ConnectionsCount == 0)
            {
                return "Пусто";
            }

            StringBuilder builder = new();

            void AddConnections(DialogProjectNodePort port)
            {
                foreach (var connection in port)
                {
                    builder.AppendLine("    " + connection.Node.ToString().Replace(Environment.NewLine, Environment.NewLine + "    "));
                }
            }

            if (TrueOutput.ConnectionsCount > 0)
            {
                builder.AppendLine("if (input)");
                builder.AppendLine("{");
                AddConnections(TrueOutput);
                builder.AppendLine("}");
            }
            if (FalseOutput.ConnectionsCount > 0)
            {
                if (TrueOutput.ConnectionsCount == 0)
                {
                    builder.AppendLine("if (input == false)");
                }
                else
                {
                    builder.AppendLine("else");
                }

                builder.AppendLine("{");
                AddConnections(FalseOutput);
                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        #endregion
    }
}

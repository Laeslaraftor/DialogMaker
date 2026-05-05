using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning;
using DialogMaker.Core.Executioning.Builders;
using System.ComponentModel;
using System.Text;

namespace DialogMaker.Core.Editor.Nodes
{
    public abstract class DialogProjectChoiceNode : DialogProjectDialogNode
    {
        public DialogProjectChoiceNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectChoiceNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) : base(dialog, savedState)
        {
        }

        [NodeInput("Вход"), Description("Вход в узел")]
        public DialogProjectNodeInputAction Input
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeInput("Говорящий"), Reference(DialogResourceType.Character)]
        public DialogProjectNodeInputCharacter Character
        {
            get
            {
                field ??= new(this, 3);
                return field;
            }
        }
        [NodeInput("Слушающий"), Reference(DialogResourceType.Character)]
        public DialogProjectNodeInputCharacter Listener
        {
            get
            {
                field ??= new(this, 4);
                return field;
            }
        }
        [NodeOutput("Выход"), Description("Действие после выбора ответа"), Sort(0)]
        public DialogProjectNodeOutputAction Output
        {
            get
            {
                field ??= new(this, 1);
                return field;
            }
        }
        [NodeOutput("Варианты ответа"), Description("Индекс выбранного варианта ответа")]
        public DialogProjectNodeOutputNumber SelectedVariantIndex
        {
            get
            {
                field ??= new(this, 2);
                return field;
            }
        }
        public override bool IsUserHandleNode => true;

        #region Управление

        public override void Compile(DialogCompilerContext context)
        {
            var character = context.Compiler.RecursiveCompileConnections(context, Character);
            var listener = context.Compiler.RecursiveCompileConnections(context, Listener);
            var variants = GetChoiceVariants();

            var outputIndex = context.Resources.GetOrCreateVariable(SelectedVariantIndex);
            var choiceOpCode = context.Section.CreateOperation(DialogByteCode.ShowChoice);

            choiceOpCode.Arguments[0] = character;
            choiceOpCode.Arguments[1] = listener;
            choiceOpCode.Arguments[2] = new(variants);
            choiceOpCode.Arguments[3] = outputIndex;

            List<DialogProjectNodeOutput> extraOutputs = [];

            foreach (var output in GetOutputs().Keys)
            {
                if (0 > output.Id && output.ConnectionsCount > 0)
                {
                    extraOutputs.Add(output);
                }
            }

            OperationBuilder? CompileOutput(DialogProjectNodeOutput extraOutput, bool onlyThreads = false, bool skipOther = false)
            {
                // дополнительные порты имеют отрицательные идентификаторы, начиная с -1
                // чтобы получить порядковый номер ответа, надо сделать небольшой фишкалик
                int id = (extraOutput.Id * -1) - 1;

                var comparison = context.Section.CreateOperation(DialogByteCode.NotEquals);
                comparison.Arguments[0] = outputIndex;
                comparison.Arguments[1] = new(id);
                var gotoOpCode = context.Section.CreateOperation(DialogByteCode.GotoIfTrue);

                context.CompileOutputs(extraOutput, onlyThreads, extraOutput.ConnectionsCount == 1);
                OperationBuilder? skipOtherOperation = null;

                if (skipOther)
                {
                    skipOtherOperation = context.Section.CreateOperation(DialogByteCode.Goto);
                }

                var emptyOpCode = context.Section.CreateOperation(DialogByteCode.Empty);
                gotoOpCode.Arguments[0] = new(emptyOpCode);

                return skipOtherOperation;
            }

            if (extraOutputs.Count == 1)
            {
                CompileOutput(extraOutputs[0], Output.ConnectionsCount > 0 || SelectedVariantIndex.ConnectionsCount > 0);
            }
            else if (extraOutputs.Count > 1)
            {
                bool onlyThreads = Output.ConnectionsCount > 0 || SelectedVariantIndex.ConnectionsCount > 0;
                List<OperationBuilder> skipOperations = new(extraOutputs.Count);

                foreach (var extraOutput in extraOutputs)
                {
                    var skipOperation = CompileOutput(extraOutput, onlyThreads, true);
                    skipOperations.Add(skipOperation!);
                }

                OperationBuilder? emptyOpCode = null;

                if (context.Section.Operations.Count > 0 &&
                    context.Section.Operations[^1].Code == DialogByteCode.Empty)
                {
                    emptyOpCode = context.Section.Operations[^1];
                }

                emptyOpCode ??= context.Section.CreateOperation(DialogByteCode.Empty);

                foreach (var skipOpCode in skipOperations)
                {
                    skipOpCode.Arguments[0] = new(emptyOpCode);
                }
            }

            context.CompileOutputs(Output);
            context.CompileOutputs(SelectedVariantIndex);
        }

        public override string ToString()
        {
            var variants = GetChoiceVariants();
            var characterReference = Character;
            StringBuilder builder = new();

            builder.AppendLine($"{Character.GetPreview()}:");

            if (variants.Strings.Count == 0)
            {
                builder.AppendLine("Пустой выбор");
                return builder.ToString();
            }

            int index = 0;

            foreach (var variant in variants.Strings)
            {
                builder.AppendLine($"[{index}] {variant.Text}");
                index++;
            }

            return builder.ToString();
        }

        protected abstract IStringCollection GetChoiceVariants();

        #endregion
    }
}

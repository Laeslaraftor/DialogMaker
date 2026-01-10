using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning;
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

        [Name("Персонаж"), Reference(DialogResourceType.Character), Sort(0)]
        public DialogProjectReference<DialogProjectCharacter>? Character
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Character));
                    field = value;
                    InvokePropertyChanged(nameof(Character));
                }
            }
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
        [NodeOutput("Выход"), Description("Действие после выбора ответа")]
        public DialogProjectNodeOutput Output
        {
            get
            {
                field ??= new(this, 1, DialogNodePortType.Action);
                return field;
            }
        }
        [NodeOutput("Варианты ответа"), Description("Индекс выбранного варианта ответа")]
        public DialogProjectNodeOutput SelectedVarianIndex
        {
            get
            {
                field ??= new(this, 2, DialogNodePortType.Number);
                return field;
            }
        }
        public override bool IsUserHandleNode => true;

        #region Управление

        public override void Compile(DialogCompilerContext context)
        {
            IResourceItem? character = Character?.Resolve();
            var variants = GetChoiceVariants();

            var outputIndex = context.Resources.GetOrCreateVariable(SelectedVarianIndex);
            var choiceOpCode = context.Section.CreateOperation(DialogByteCode.ShowChoice);
            
            if (character != null)
            {
                choiceOpCode.Arguments[0] = new(character);
            }

            choiceOpCode.Arguments[1] = new(variants);
            choiceOpCode.Arguments[2] = outputIndex;

            context.CompileOutputs(Output);
            context.CompileOutputs(SelectedVarianIndex);
        }

        public override string ToString()
        {
            var variants = GetChoiceVariants();
            var characterReference = Character;
            StringBuilder builder = new();
            
            if (characterReference != null)
            {
                builder.AppendLine($"{characterReference.Resolve()}:");
            }
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

        protected override DialogProjectDialogNodeSavedState CreateSavedState()
        {
            var savedState = base.CreateSavedState();
            savedState.Properties.TryAdd(nameof(Character), Character?.Save());

            return savedState;
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);

            Character = savedState.RestoreReference<DialogProjectCharacter>(Project, nameof(Character));
        }

        #endregion
    }
}

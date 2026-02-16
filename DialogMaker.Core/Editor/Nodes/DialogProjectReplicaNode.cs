using DialogMaker.Core.Attributes;
using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning;
using DialogMaker.Core.Executioning.Builders;
using System.Linq.Expressions;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectReplicaNode : DialogProjectDialogNode
    {
        public DialogProjectReplicaNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectReplicaNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.SimpleReplica;
        [Name("Говорящий"), Reference(DialogResourceType.Character)]
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
        [Name("Слушающий"), Reference(DialogResourceType.Character)]
        public DialogProjectReference<DialogProjectCharacter>? Listener
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Listener));
                    field = value;
                    InvokePropertyChanged(nameof(Listener));
                }
            }
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
        [NodeInput("Говорящий"), Reference(DialogResourceType.Character)]
        public DialogProjectNodeInputReference CharacterInput
        {
            get
            {
                field ??= new(this, 3, DialogResourceType.Character);
                return field;
            }
        }
        [NodeInput("Текст")]
        public DialogProjectNodeInputString Text
        {
            get
            {
                field ??= new(this, 2);
                return field;
            }
        }
        [NodeOutput("Выход")]
        public DialogProjectNodeOutput Output
        {
            get
            {
                field ??= new(this, 1, DialogNodePortType.Action);
                return field;
            }
        }
        public override bool IsUserHandleNode => true;

        #region Управление

        public override void Compile(DialogCompilerContext context)
        {
            var text = context.Compiler.RecursiveCompileConnections(context, Text);
            var character = Character?.Resolve();
            var listener = Listener?.Resolve();

            CreateOperation(context, character, listener, text, DialogByteCode.ShowReplica, DialogByteCode.ShowResourceReplica);
            context.CompileOutputs(Output);
        }

        public override string ToString()
        {
            string characterName = string.Empty;
            var character = Character;

            if (character != null)
            {
                characterName = $"{character.Resolve()}: ";
            }

            return $"{characterName}{Text.GetPreview()}";
        }

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);
            savedState.Properties.TryAdd(nameof(Character), Character?.Save());
            savedState.Properties.TryAdd(nameof(Listener), Listener?.Save());
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);
            Character = savedState.RestoreReference<DialogProjectCharacter>(Project, nameof(Character));
            Listener = savedState.RestoreReference<DialogProjectCharacter>(Project, nameof(Listener));
        }

        #endregion

        #region Статика

        internal static OperationBuilder CreateOperation(DialogCompilerContext context, ICharacter? character, ICharacter? listener, DialogExecutionParameter text, DialogByteCode original, DialogByteCode resource)
        {
            DialogByteCode code = original;

            if (text.Value is IResourceItem)
            {
                code = resource;
            }

            var result = context.Section.CreateOperation(code);

            if (character != null)
            {
                result.Arguments[0] = new(character);
            }
            if (listener != null)
            {
                result.Arguments[1] = new(listener);
            }

            result.Arguments[2] = text;

            return result;
        }

        #endregion
    }
}

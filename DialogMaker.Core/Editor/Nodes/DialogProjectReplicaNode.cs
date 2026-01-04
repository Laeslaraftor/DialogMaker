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
        [Name("Персонаж"), Reference(DialogResourceType.Character)]
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
        [NodeInput("Вход")]
        public DialogProjectNodeInputAction Input
        {
            get
            {
                field ??= new(this, 0);
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

        #region Управление

        public override void Compile(DialogCompilerContext context)
        {
            var text = context.Compiler.RecursiveCompileConnections(context, Text);
            var character = Character?.Resolve();

            CreateOperation(context, character, text, DialogByteCode.ShowReplica, DialogByteCode.ShowResourceReplica);
            context.Compiler.CompileOutputs(context, Output);
        }

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);
            savedState.Properties.TryAdd(nameof(Character), Character?.Save());
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);
            Character = savedState.RestoreReference<DialogProjectCharacter>(Project, nameof(Character));
        }

        #endregion

        #region Статика

        internal static OperationBuilder CreateOperation(DialogCompilerContext context, ICharacter? character, DialogExecutionParameter text, DialogByteCode original, DialogByteCode resource)
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

            result.Arguments[1] = text;

            return result;
        }

        #endregion
    }
}

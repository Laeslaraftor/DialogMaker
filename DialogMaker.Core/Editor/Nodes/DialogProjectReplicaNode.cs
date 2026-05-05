using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning;
using DialogMaker.Core.Executioning.Builders;

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
        public DialogProjectNodeOutputAction Output
        {
            get
            {
                field ??= new(this, 1);
                return field;
            }
        }
        public override bool IsUserHandleNode => true;

        #region Управление

        public override void Compile(DialogCompilerContext context)
        {
            var text = context.Compiler.RecursiveCompileConnections(context, Text);
            var character = context.Compiler.RecursiveCompileConnections(context, Character);
            var listener = context.Compiler.RecursiveCompileConnections(context, Listener);

            CreateOperation(context, character, listener, text, DialogByteCode.ShowReplica, DialogByteCode.ShowResourceReplica);
            context.CompileOutputs(Output);
        }

        public override string ToString()
        {
            return $"{Character.GetPreview()}: {Text.GetPreview()}";
        }

        #endregion

        #region Статика

        internal static OperationBuilder CreateOperation(DialogCompilerContext context, DialogExecutionParameter character, DialogExecutionParameter listener, DialogExecutionParameter text, DialogByteCode original, DialogByteCode resource)
        {
            DialogByteCode code = original;

            if (text.Value is IResourceItem)
            {
                code = resource;
            }

            var result = context.Section.CreateOperation(code);

            if (character != null)
            {
                result.Arguments[0] = character;
            }
            if (listener != null)
            {
                result.Arguments[1] = listener;
            }

            result.Arguments[2] = text;

            return result;
        }

        #endregion
    }
}

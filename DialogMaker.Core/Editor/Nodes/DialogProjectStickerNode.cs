using DialogMaker.Core.Executioning;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectStickerNode : DialogProjectDialogNode
    {
        public DialogProjectStickerNode(DialogProjectDialog dialog)
            : base(dialog)
        {
        }
        public DialogProjectStickerNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Sticker;
        public override bool IsCodeGenerator => false;
        [Name("Текст"), Text(AllowMultiline = true)]
        public string? Text
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Text));
                    field = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
        }

        #region Управление

        public override void Compile(DialogCompilerContext context)
        {
        }

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);
            savedState.Properties.TryAdd(nameof(Text), Text);
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);

            if (savedState.Properties.TryGetValue(nameof(Text), out var text))
            {
                Text = text?.ToString();
            }
        }

        #endregion
    }
}

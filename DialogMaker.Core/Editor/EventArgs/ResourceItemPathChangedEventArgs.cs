namespace DialogMaker.Core.Editor
{
    public readonly struct ResourceItemPathChangedEventArgs(DialogProjectResourceObject item, DialogProjectResources oldResources, ResourcePath oldPath)
    {
        public DialogProjectResourceObject Item { get; } = item;
        public DialogProjectResources OldResources { get; } = oldResources;
        public DialogProjectResources NewResources { get; } = item.Resources;
        public ResourcePath OldPath { get; } = oldPath;
        public ResourcePath NewPath { get; } = item.Path;
    }
}

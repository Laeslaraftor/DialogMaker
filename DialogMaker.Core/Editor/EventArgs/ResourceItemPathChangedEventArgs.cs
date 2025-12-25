namespace DialogMaker.Core.Editor
{
    public readonly struct ResourceItemPathChangedEventArgs(DialogProjectResourceObject item, DialogProjectResources oldResources, string oldPath)
    {
        public DialogProjectResourceObject Item { get; } = item;
        public DialogProjectResources OldResources { get; } = oldResources;
        public DialogProjectResources NewResources { get; } = item.Resources;
        public string OldPath { get; } = oldPath;
        public string NewPath { get; } = item.Path;
    }
}

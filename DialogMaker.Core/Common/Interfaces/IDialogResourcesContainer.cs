namespace DialogMaker.Core.Common
{
    public interface IDialogResourcesContainer : IResourcesOwner
    {
        public DialogPackage Package { get; }
        public string Folder { get; }
        public new DialogResources Resources { get; }
    }
}

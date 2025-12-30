namespace DialogMaker.Core.Editor
{
    public interface IProjectResourcesOwner : IResourcesOwner
    {
        public DialogProject Project { get; }
        public string Folder { get; }
        public new DialogProjectResources Resources { get; }
        public new IProjectResourcesOwner? Parent { get; }
    }
}

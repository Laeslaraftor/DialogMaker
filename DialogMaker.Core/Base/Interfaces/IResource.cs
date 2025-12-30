namespace DialogMaker.Core
{
    public interface IResource
    {
        public IResourcesContainer Container { get; }
        public string Id { get; }
        public DialogResourceType ResourceType { get; }
    }
}

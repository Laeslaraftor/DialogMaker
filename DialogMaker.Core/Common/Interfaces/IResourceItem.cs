namespace DialogMaker.Core.Common
{
    public interface IResourceItem
    {
        public DialogResourceType ResourceType { get; }
        public string Id { get; }
    }
}

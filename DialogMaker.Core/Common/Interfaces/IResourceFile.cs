namespace DialogMaker.Core.Common
{
    public interface IResourceFile : IResourceItem
    {
        public DialogFileResourceType Type { get; }
        public string FilePath { get; }
    }
}

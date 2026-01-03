namespace DialogMaker.Core.Common
{
    public interface IResourceString : IResourceItem
    {
        public string Text { get; }
        public IResourceFile? Voice { get; }
    }
}

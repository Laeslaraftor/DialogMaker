using DialogMaker.Core.Common;

namespace DialogMaker.Core
{
    public interface IResource : IResourceItem
    {
        public IResourcesContainer Container { get; }
    }
}

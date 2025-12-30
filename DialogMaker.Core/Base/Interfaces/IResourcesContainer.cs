using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core
{
    public interface IResourcesContainer
    {
        public IResourcesOwner Owner { get; }

        public bool TryGetResource(DialogResourceType type, string id, [NotNullWhen(true)] out IResource? result);
    }
}

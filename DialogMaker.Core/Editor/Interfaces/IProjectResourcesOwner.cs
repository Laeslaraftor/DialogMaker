using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Editor
{
    public interface IProjectResourcesOwner
    {
        public DialogProject Project { get; }
        public string Id { get; }
        public string Folder { get; }
        public DialogProjectResources Resources { get; }
        public IProjectResourcesOwner? Parent { get; }

        public bool TryGetChild(string id, [NotNullWhen(true)] out IProjectResourcesOwner? result);
    }
}

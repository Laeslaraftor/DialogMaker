using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core
{
    public interface IResourcesOwner
    {
        public IResourcesOwner Root { get; }
        public IResourcesOwner? Parent { get; }
        public IResourcesContainer Resources { get; }
        public string Id { get; }

        public bool TryFindChild(string id, [NotNullWhen(true)] out IResourcesOwner? result);

        #region Статика

        public static IResource FindResource(IResourcesOwner owner, ResourcePath path)
        {
            if (path.IsEmpty)
            {
                throw new ArgumentException($"Не возможно получить ресурс по пустому пути: {path}", nameof(path));
            }

            return FindResource(owner, path.ResourceType, path.Id, path.OwnerPath);
        }
        public static IResource FindResource(IResourcesOwner owner, DialogResourceType type, string itemId, string path)
        {
            var pathIds = path.Split('/');

            IResourcesOwner container = owner.Root;
            int index = 0;

            while (index < pathIds.Length)
            {
                var id = pathIds[index++];

                if (id == ".")
                {
                    continue;
                }
                if (!container.TryFindChild(id, out var nextContainer))
                {
                    throw new ArgumentException($"Не удалось найти владельца ресурсов с идентификатором \"{id}\" в \"{container.Id}\". Путь: {path}", nameof(path));
                }

                container = nextContainer;
            }

            if (container.Resources.TryGetResource(type, itemId, out var result))
            {
                return result;
            }

            throw new ArgumentException($"Не удалось найти ресурс с идентификатором \"{itemId}\" в \"{container.Id}\". Путь: {path}", nameof(itemId));
        }

        #endregion
    }
}

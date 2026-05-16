using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core
{
    /// <summary>
    /// Интерфейс владельца контейнера ресурсов
    /// </summary>
    public interface IResourcesOwner
    {
        /// <summary>
        /// Корневой владелец контейнера ресурсов
        /// </summary>
        public IResourcesOwner Root { get; }
        /// <summary>
        /// Родительский владелец контейнера ресурсов
        /// </summary>
        public IResourcesOwner? Parent { get; }
        /// <summary>
        /// Контейнер ресурсов
        /// </summary>
        public IResourcesContainer Resources { get; }
        /// <summary>
        /// Идентификатор владельца контейнера ресурсов
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Попытаться найти дочернего владельца контейнера ресурсов
        /// </summary>
        /// <param name="id">Идентификатор искомого владельца контейнера ресурсов</param>
        /// <param name="result">Результат поиска</param>
        /// <returns>Удалось ли найти владельца контейнера ресурсов</returns>
        public bool TryFindChild(string id, [NotNullWhen(true)] out IResourcesOwner? result);

        #region Статика

        /// <summary>
        /// Найти ресурс по пути
        /// </summary>
        /// <param name="owner">Владелец контейнера ресурсов</param>
        /// <param name="path">Путь к ресурсу</param>
        /// <returns>Найденный ресурс</returns>
        /// <exception cref="ArgumentException">Невозможно получить ресурс по пустому пути</exception>
        public static IResource FindResource(IResourcesOwner owner, ResourcePath path)
        {
            if (path.IsEmpty)
            {
                throw new ArgumentException($"Невозможно получить ресурс по пустому пути: {path}", nameof(path));
            }

            return FindResource(owner, path.ResourceType, path.Id, path.OwnerPath);
        }
        /// <summary>
        /// Найти ресурс
        /// </summary>
        /// <param name="owner">Владелец контейнера ресурсов</param>
        /// <param name="type">Тип искомого ресурса</param>
        /// <param name="itemId">Идентификатор искомого ресурса</param>
        /// <param name="path">Путь к ресурса</param>
        /// <returns>Найденный ресурс</returns>
        /// <exception cref="ArgumentException">Не удалось найти владельца ресурсов</exception>
        /// <exception cref="ArgumentException">Не удалось найти ресурс</exception>
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

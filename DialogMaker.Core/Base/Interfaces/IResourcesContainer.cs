using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core
{
    /// <summary>
    /// Интерфейс контейнера ресурсов
    /// </summary>
    public interface IResourcesContainer
    {
        /// <summary>
        /// Владелец контейнера ресурсов
        /// </summary>
        public IResourcesOwner Owner { get; }

        /// <summary>
        /// Попытаться получить ресурс
        /// </summary>
        /// <param name="type">Тип ресурса</param>
        /// <param name="id">Идентификатор ресурса</param>
        /// <param name="result">Результат поиска</param>
        /// <returns>Удалось ли получить ресурс</returns>
        public bool TryGetResource(DialogResourceType type, string id, [NotNullWhen(true)] out IResource? result);
    }
}

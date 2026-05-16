using DialogMaker.Core.Common;

namespace DialogMaker.Core
{
    /// <summary>
    /// Интерфейс ресурса, содержащегося в контейнере ресурсов
    /// </summary>
    public interface IResource : IResourceItem
    {
        /// <summary>
        /// Контейнер ресурсов в котором содержится текущий ресурс
        /// </summary>
        public IResourcesContainer Container { get; }
    }
}

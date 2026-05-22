using DialogMaker.Core.Executioning;

namespace DialogMaker.Core.Common
{
    /// <summary>
    /// Интерфейс ресурса
    /// </summary>
    public interface IResourceItem
    {
        /// <summary>
        /// Тип ресурса
        /// </summary>
        public DialogResourceType ResourceType { get; }
        /// <summary>
        /// Идентификатор ресурса
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Является ли ресурс самостоятельным.
        /// Самостоятельные ресурсы существуют независимо от контейнеров ресурсов и,
        /// как правило, у таких ресурсов невозможно получить путь
        /// </summary>
        public bool IsSeparated { get; }

        #region Управление

        /// <summary>
        /// Получить ссылку на ресурс
        /// </summary>
        /// <returns>Ссылка на ресурс</returns>
        public DialogItemReference CreateReference();
        /// <summary>
        /// Получить путь к ресурсу. Если ресурс является самостоятельным, то путь вряд ли получиться получить
        /// </summary>
        /// <returns>Путь к ресурсу</returns>
        /// <exception cref="InvalidOperationException">Получение пути недоступно при <see cref="IsSeparated"/> установленном как True</exception>
        public ResourcePath GetPath();
        /// <summary>
        /// Преобразовать ресурс в переменную
        /// </summary>
        /// <returns>Ресурс как переменная</returns>
        public IVariable ToVariable();

        #endregion

        #region Константы

        /// <summary>
        /// Текст исключения, возникающего при попытке получить путь у самостоятельного ресурса
        /// </summary>
        public const string GetPathExceptionMessage = $"Получение пути недоступно при {nameof(IsSeparated)} установленном как True";

        #endregion
    }
}

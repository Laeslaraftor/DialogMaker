namespace DialogMaker.Core.Common
{
    /// <summary>
    /// Интерфейс владельца контейнера ресурсов
    /// </summary>
    public interface IDialogResourcesContainer : IResourcesOwner
    {
        /// <summary>
        /// Пакет диалогов
        /// </summary>
        public DialogPackage Package { get; }
        /// <summary>
        /// Путь к папке в котором находится контейнер ресурсов
        /// </summary>
        public string Folder { get; }
        /// <summary>
        /// Контейнер ресурсов
        /// </summary>
        public new DialogResources Resources { get; }
    }
}

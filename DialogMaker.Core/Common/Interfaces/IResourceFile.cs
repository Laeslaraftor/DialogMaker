namespace DialogMaker.Core.Common
{
    /// <summary>
    /// Интерфейс ресурса файла
    /// </summary>
    public interface IResourceFile : IResourceItem
    {
        /// <summary>
        /// Тип файла
        /// </summary>
        public DialogFileResourceType Type { get; }
        /// <summary>
        /// Путь к файлу
        /// </summary>
        public string FilePath { get; }
    }
}

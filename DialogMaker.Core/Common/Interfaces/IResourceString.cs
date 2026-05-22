namespace DialogMaker.Core.Common
{
    /// <summary>
    /// Интерфейс ресурса строки
    /// </summary>
    public interface IResourceString : IResourceItem
    {
        /// <summary>
        /// Значение строки
        /// </summary>
        public string Text { get; }
        /// <summary>
        /// Ресурс озвучки строки
        /// </summary>
        public IResourceFile? Voice { get; }
    }
}

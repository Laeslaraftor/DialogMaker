namespace DialogMaker.Core.Common
{
    /// <summary>
    /// Интерфейс ресурса персонажа
    /// </summary>
    public interface ICharacter : IResourceItem
    {
        /// <summary>
        /// Имя персонажа
        /// </summary>
        public string Name { get; }
    }
}

using System.Collections.ObjectModel;

namespace DialogMaker.Core.Common
{
    /// <summary>
    /// Интерфейс ресурса коллекции строк
    /// </summary>
    public interface IStringCollection : IResourceItem
    {
        /// <summary>
        /// Коллекция строк
        /// </summary>
        public ReadOnlyCollection<IResourceString> Strings { get; }
    }
}

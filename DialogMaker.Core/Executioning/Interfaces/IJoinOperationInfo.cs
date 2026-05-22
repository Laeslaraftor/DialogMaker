using DialogMaker.Core.Common;
using System.Collections.ObjectModel;

namespace DialogMaker.Core.Executioning
{
    /// <summary>
    /// Интерфейс информации об операции над потоками
    /// </summary>
    public interface IJoinOperationInfo : IResourceItem
    {
        /// <summary>
        /// Позиции с которых продолжится выполнение кода
        /// </summary>
        public ReadOnlyCollection<DialogPosition> Outputs { get; }
        /// <summary>
        /// Сегменты кода из которых идёт вход в операцию
        /// </summary>
        public ReadOnlyCollection<int> InputSections { get; }
    }
}

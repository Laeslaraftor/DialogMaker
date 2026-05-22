using DialogMaker.Core.Common;

namespace DialogMaker.Core.Executioning
{
    /// <summary>
    /// Тип объекта диалога
    /// </summary>
    public enum DialogItemType
    {
        /// <summary>
        /// Ресурс (<see cref="IResourceItem"/>)
        /// </summary>
        Resource,
        /// <summary>
        /// Переменная (<see cref="IVariable"/>)
        /// </summary>
        Variable,
        /// <summary>
        /// Строка (<see cref="IResourceString"/>)
        /// </summary>
        String,
        /// <summary>
        /// Коллекция строк (<see cref="IStringCollection"/>)
        /// </summary>
        StringCollection,
        /// <summary>
        /// Информация об операции с потоками (<see cref="IJoinOperationInfo"/>)
        /// </summary>
        JoinInfo,
        /// <summary>
        /// Персонаж (<see cref="ICharacter"/>)
        /// </summary>
        Character,
        /// <summary>
        /// Событие (<see cref="Executioning.Trigger"/>)
        /// </summary>
        Trigger
    }
}

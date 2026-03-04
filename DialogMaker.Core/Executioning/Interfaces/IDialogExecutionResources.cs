using DialogMaker.Core.Common;

namespace DialogMaker.Core.Executioning
{
    /// <summary>
    /// Интерфейс ресурсов диалога во время выполнения
    /// </summary>
    public interface IDialogExecutionResources
    {
        /// <summary>
        /// Получить ресурс по ссылке
        /// </summary>
        /// <param name="reference">Ссылка на ресурс</param>
        /// <returns>Ресурс, полученный по ссылке</returns>
        public IResourceItem GetItemFromReference(DialogItemReference reference);

        /// <summary>
        /// Получить ресурс (<see cref="IResourceString"/>, <see cref="IVariable"/>, <see cref="ICharacter"/>, <see cref="IEmotion"/>, <see cref="IStringCollection"/>) по указанному индексу
        /// </summary>
        /// <param name="index">Индекс ресурса</param>
        /// <returns>Ресурс, полученный по указанному индексу</returns>
        public IResourceItem GetResource(int index);
        /// <summary>
        /// Получить значение переменной по указанному индексу.
        /// Если по указанному индексу нет переменной, то будет возвращено 0.
        /// </summary>
        /// <param name="index">Индекс переменной</param>
        /// <returns>Значение переменной по указанному индексу</returns>
        public OperandValue GetVariable(int index);
        /// <summary>
        /// Задать значение индексу
        /// </summary>
        /// <param name="index">Индекс, которому надо задать значение</param>
        /// <param name="value">Значение переменной</param>
        public void SetValue(int index, OperandValue value);
        /// <summary>
        /// Задать значение индексу
        /// </summary>
        /// <param name="index">Индекс, которому надо задать значение</param>
        /// <param name="resource">Ресурс, который будет задан индексу</param>
        public void SetValue(int index, IResourceItem resource);

        /// <summary>
        /// Сбросить значения
        /// </summary>
        public void Reset();
    }
}

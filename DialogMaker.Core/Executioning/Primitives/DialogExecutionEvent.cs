using DialogMaker.Core.Editor.Nodes;

namespace DialogMaker.Core.Executioning
{
    /// <summary>
    /// Событие выполнения диалога
    /// </summary>
    public enum DialogExecutionEvent : byte
    {
        /// <summary>
        /// Событие запуска диалога
        /// </summary>
        [Name("Запуск")]
        Started,
        /// <summary>
        /// Событие приостановки диалога
        /// </summary>
        [Name("Пауза")]
        Paused,
        /// <summary>
        /// Событие возобновление диалога
        /// </summary>
        [Name("Возобновление")]
        Resumed,
        /// <summary>
        /// Событие завершения диалога
        /// </summary>
        [Name("Завершение")]
        Completing
    }
}

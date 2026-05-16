namespace DialogMaker.Core
{
    /// <summary>
    /// Прогресс получения результата
    /// </summary>
    /// <typeparam name="T">Тип результата</typeparam>
    public struct ProgressResult<T>
    {
        /// <summary>
        /// Завершён ли процесс
        /// </summary>
        public bool IsCompleted { get; set; }
        /// <summary>
        /// Общий прогресс выполнения
        /// </summary>
        public float Progress { get; set; }
        /// <summary>
        /// Локальный прогресс выполнения
        /// </summary>
        public float LocalProgress { get; set; }
        /// <summary>
        /// Полученный результат
        /// </summary>
        public T Value { get; set; }
        /// <summary>
        /// Дополнительные данные
        /// </summary>
        public object? Extra { get; set; }
    }
}

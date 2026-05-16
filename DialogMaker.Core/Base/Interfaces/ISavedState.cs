namespace DialogMaker.Core
{
    /// <summary>
    /// Интерфейс сохранённого состояния объекта
    /// </summary>
    public interface ISavedState
    {
        /// <summary>
        /// Сохранить состояние в файл
        /// </summary>
        /// <param name="filePath">Путь к файлу в который будет записано состояние</param>
        public void Save(string filePath);
    }
}

namespace DialogMaker.Core
{
    /// <summary>
    /// Интерфейс сохраняемого объекта
    /// </summary>
    public interface ISavable
    {
        /// <summary>
        /// Сохранить объект и получить сохранённое состояние
        /// </summary>
        /// <returns>Сохранённое состояние объекта</returns>
        public ISavedState Save();
    }
}

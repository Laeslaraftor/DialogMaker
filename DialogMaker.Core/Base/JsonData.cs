using Newtonsoft.Json;

namespace DialogMaker.Core
{
    /// <summary>
    /// Базовый класс сохранённого состояния в json формате
    /// </summary>
    public class JsonData : ISavedState
    {
        /// <summary>
        /// Расширение json файла
        /// </summary>
        public const string FileExtension = "json";
        /// <summary>
        /// Фильтр json файлов для окна выбора файлов
        /// </summary>
        public const string FileFilter = $"Json files (.{FileExtension})|*.{FileExtension}";

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="filePath"><inheritdoc/></param>
        public void Save(string filePath)
        {
            string json = JsonConvert.SerializeObject(this);
            File.WriteAllText(filePath, json);
        }

        #endregion
    }
}

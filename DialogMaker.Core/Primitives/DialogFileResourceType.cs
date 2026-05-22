using DialogMaker.Core.Editor.Nodes;

namespace DialogMaker.Core
{
    /// <summary>
    /// Тип ресурса файла
    /// </summary>
    public enum DialogFileResourceType
    {
        /// <summary>
        /// Аудиофайл
        /// </summary>
        [Name("Аудио"), Icon("\uE8D6")]
        Audio,
        /// <summary>
        /// Видео
        /// </summary>
        [Name("Видео"), Icon("\uE8B2")]
        Video,
        /// <summary>
        /// Изображение
        /// </summary>
        [Name("Изображение"), Icon("\uE91B")]
        Image
    }
}

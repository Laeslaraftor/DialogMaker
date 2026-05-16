using DialogMaker.Core.Attributes;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;

namespace DialogMaker.Core
{
    /// <summary>
    /// Тип ресурса
    /// </summary>
    public enum DialogResourceType
    {
        /// <summary>
        /// Строка
        /// </summary>
        [Name("Строка"), ResourceType(typeof(DialogProjectString), IsDev = true)]
        String,
        /// <summary>
        /// Персонаж
        /// </summary>
        [Name("Персонаж"), ResourceType(typeof(DialogProjectCharacter), IsDev = true)]
        Character,
        /// <summary>
        /// Файл (аудио, видео, изображение)
        /// </summary>
        [Name("Файл"), ResourceType(typeof(DialogProjectItem), IsDev = true)]
        File,
        /// <summary>
        /// Переменная
        /// </summary>
        [Name("Переменная"), ResourceType(typeof(DialogProjectVariable), IsDev = true)]
        Variable,
        /// <summary>
        /// Эмоция
        /// </summary>
        [Name("Эмоция"), ResourceType(typeof(DialogProjectEmotion), IsDev = true)]
        Emotion,
        /// <summary>
        /// Шаблон события
        /// </summary>
        [Name("Шаблон события"), ResourceType(typeof(DialogProjectTriggerPreset), IsDev = true)]
        TriggerPreset,
    }
}

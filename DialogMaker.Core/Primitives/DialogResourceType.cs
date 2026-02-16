using DialogMaker.Core.Attributes;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;

namespace DialogMaker.Core
{
    public enum DialogResourceType
    {
        [Name("Строка"), ResourceType(typeof(DialogProjectString), IsDev = true)]
        String,
        [Name("Персонаж"), ResourceType(typeof(DialogProjectCharacter), IsDev = true)]
        Character,
        [Name("Файл"), ResourceType(typeof(DialogProjectItem), IsDev = true)]
        File,
        [Name("Переменная"), ResourceType(typeof(DialogProjectVariable), IsDev = true)]
        Variable,
        [Name("Эмоция"), ResourceType(typeof(DialogProjectEmotion), IsDev = true)]
        Emotion,
    }
}

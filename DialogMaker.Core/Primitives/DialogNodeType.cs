using System.ComponentModel;

namespace DialogMaker.Core
{
    public enum DialogNodeType
    {
        [Description("Самая обычная реплика персонажа")]
        SimpleReplica,
        [Description("Выбор ответа")]
        Choice,
        [Description("Реплика на фоне картинки, видео или залитого цветом экрана")]
        FullScreenReplica,
        [Description("Текст в центре залитого цветом экрана")]
        FullScreenScreenText
    }
}

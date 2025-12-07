using DialogMaker.Core.Editor.Nodes;
using System.ComponentModel;

namespace DialogMaker.Core
{
    public enum DialogNodeType
    {
        [Name("Реплика"), Description("Самая обычная реплика персонажа"), Node(typeof(DialogProjectReplicaNode))]
        SimpleReplica,
        [Name("Вариант ответа"), Description("Выбор ответа"), Node(typeof(DialogProjectChoiceNode))]
        Choice,
        [Name("Полноэкранная реплика"), Description("Реплика на фоне картинки, видео или залитого цветом экрана")]
        FullScreenReplica,
        [Name("Полноэкранный текст"), Description("Текст в центре залитого цветом экрана")]
        FullScreenScreenText,
        [Name("Точка входа"), Node(typeof(DialogProjectStartNode))]
        Start,
        [Name("Завершение"), Node(typeof(DialogProjectEndNode))]
        End
    }
}

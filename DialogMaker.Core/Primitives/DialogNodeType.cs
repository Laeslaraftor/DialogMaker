using DialogMaker.Core.Editor.Nodes;
using System.ComponentModel;

namespace DialogMaker.Core
{
    public enum DialogNodeType
    {
        // Диалог

        [Name("Реплика"), Node(typeof(DialogProjectReplicaNode))]
        [Description("Самая обычная реплика персонажа.")]
        [Path("Диалог")]
        SimpleReplica,

        [Name("Вариант ответа (ресурсы)"), Node(typeof(DialogProjectResourceChoiceNode))]
        [Description("Выбор ответа. В качестве вариантов ответа задаются строковые ресурсы.")]
        [Path("Диалог")]
        Choice,

        [Name("Полноэкранная реплика")]
        [Description("Реплика на фоне картинки, видео или залитого цветом экрана.")]
        [Path("Диалог")]
        FullScreenReplica,

        [Name("Полноэкранный текст")]
        [Description("Текст в центре залитого цветом экрана.")]
        [Path("Диалог")]
        FullScreenScreenText,

        [Name("Событие"), Node(typeof(DialogProjectTriggerNode))]
        [Description("Вызвать событие. Событие будет получено обработчиком диалога.")]
        [Path("Диалог")]
        Trigger,

        [Name("Завершение"), Node(typeof(DialogProjectEndNode))]
        [Description("Завершить текущий диалог.")]
        [Path("Диалог")]
        End,

        // Логика

        [Name("Сравнение"), Node(typeof(DialogProjectCompareNode))]
        [Description("Сравнить данные между собой.")]
        [Path("Логика")]
        Compare,

        [Name("Выбрать данные"), Node(typeof(DialogProjectSelectIfNode))]
        [Description("Выбрать данные в зависимости от входного значения. 0 = false, остальное - true.")]
        [Path("Логика")]
        SelectIf,

        [Name("Ветвление"), Node(typeof(DialogProjectDoIfNode))]
        [Description("Выполнить следующее действие в зависимости от входного значения. 0 = false, остальное - true.")]
        [Path("Логика")]
        DoIf,

        // Вычисления

        [Name("Сложить"), Node(typeof(DialogProjectAddNode))]
        [Description("Сложить данные.")]
        [Path("Вычисления")]
        Add,

        [Name("Вычесть"), Node(typeof(DialogProjectSubtractNode))]
        [Description("Вычесть данные.")]
        [Path("Вычисления")]
        Subtract,

        [Name("Умножить"), Node(typeof(DialogProjectMultiplyNode))]
        [Description("Умножить данные.")]
        [Path("Вычисления")]
        Multiply,

        [Name("Разделить"), Node(typeof(DialogProjectDivideNode))]
        [Description("Разделить данные.")]
        [Path("Вычисления")]
        Divide,

        [Name("Заменить"), Node(typeof(DialogProjectReplaceNode))]
        [Description("Заменить данные.")]
        [Path("Вычисления")]
        Replace,

        // Данные

        [Name("Переменная"), Node(typeof(DialogProjectVariableNode))]
        [Description("Переменная проекта, набора или диалога.")]
        [Path("Данные")]
        Variable,

        [Name("Ресурс"), Node(typeof(DialogProjectReferenceNode))]
        [Description("Ресурс проекта, набора или диалога.")]
        [Path("Данные")]
        Reference,

        [Name("Строка"), Node(typeof(DialogProjectStringNode))]
        [Description("Любая строка.")]
        [Path("Данные")]
        String,

        [Name("Число"), Node(typeof(DialogProjectNumberNode))]
        [Description("Любое число.")]
        [Path("Данные")]
        Number,

        [Name("Вариант ответа"), Node(typeof(DialogProjectSimpleChoiceNode))]
        [Description("Выбор ответа.")]
        [Path("Диалог")]
        SimpleChoice,

        [Name("Случайное число"), Node(typeof(DialogProjectRandomNode))]
        [Description("Случайное число в указанном диапазоне.")]
        [Path("Данные")]
        RandomNumber,

        [Name("Состояние диалога"), Node(typeof(DialogProjectEventNode))]
        [Description("Выполнить действия при вызове определённого события диалога.")]
        [Path("Диалог")]
        Event,

        [Name("Ждать"), Node(typeof(DialogProjectWaitNode))]
        [Description("Подождать определённое количество времени.")]
        [Path("Поток")]
        Wait,

        [Name("Объединить"), Node(typeof(DialogProjectJoinNode))]
        [Description("Ожидать завершение всех входящих потоков, а затем объединить их в один.")]
        [Path("Поток")]
        Join,

        [Name("Только первый вход"), Node(typeof(DialogProjectIntersectNode))]
        [Description("Принять первый поток и продолжить его выполнение, завершая все последующие входящие потоки.")]
        [Path("Поток")]
        Intersect,

        [Name("Минимум"), Node(typeof(DialogProjectMinimumNode))]
        [Description("Выбрать минимальное из двух входящих значений.")]
        [Path("Вычисления")]
        Minimum,

        [Name("Максимум"), Node(typeof(DialogProjectMaximumNode))]
        [Description("Выбрать максимальное из двух входящих значений.")]
        [Path("Вычисления")]
        Maximum,
    }
}

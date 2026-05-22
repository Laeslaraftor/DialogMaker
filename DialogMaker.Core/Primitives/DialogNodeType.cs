using DialogMaker.Core.Editor.Nodes;
using System.ComponentModel;

namespace DialogMaker.Core
{
    /// <summary>
    /// Тип узла диалога
    /// </summary>
    public enum DialogNodeType
    {
        // Диалог

        /// <summary>
        /// Реплика
        /// </summary>
        [Name("Реплика"), Node(typeof(DialogProjectReplicaNode))]
        [Description("Самая обычная реплика персонажа.")]
        [Path("Диалог")]
        [Tags("replica", "реплика", "htgkbrf")]
        SimpleReplica,
        /// <summary>
        /// Выбор ответа
        /// </summary>
        [Obsolete]
        [Name("Вариант ответа (ресурсы)")]
        [Description("Выбор ответа. В качестве вариантов ответа задаются строковые ресурсы.")]
        [Path("Диалог")]
        [Tags("choice", "выбор", "ответы", "ds,jh", "jndtns")]
        Choice,
        /// <summary>
        /// Полноэкранная реплика
        /// </summary>
        [Obsolete]
        [Name("Полноэкранная реплика")]
        [Description("Реплика на фоне картинки, видео или залитого цветом экрана.")]
        [Path("Диалог")]
        FullScreenReplica,
        /// <summary>
        /// Полноэкранный текст
        /// </summary>
        [Obsolete]
        [Name("Полноэкранный текст")]
        [Description("Текст в центре залитого цветом экрана.")]
        [Path("Диалог")]
        FullScreenScreenText,
        /// <summary>
        /// Событие
        /// </summary>
        [Name("Событие"), Node(typeof(DialogProjectTriggerNode))]
        [Description("Вызвать событие. Событие будет получено обработчиком диалога.")]
        [Path("Диалог")]
        [Tags("trigger", "event", "callback", "function", "событие", "триггер", "вызов", "функция")]
        [Tags("cj,snbt", "nhbuuth", "dspjd", "aeyrwbz")]
        Trigger,
        /// <summary>
        /// Принудительное завершение диалога
        /// </summary>
        [Name("Завершение"), Node(typeof(DialogProjectEndNode))]
        [Description("Завершить текущий диалог или поток.")]
        [Path("Диалог")]
        [Tags("end", "complete", "stop", "конец", "завершение", "остановить")]
        [Tags("rjytw", "pfdthitybt", "jcnfyjdbnm")]
        End,

        // Логика
        /// <summary>
        /// Сравнение данных
        /// </summary>
        [Name("Сравнение"), Node(typeof(DialogProjectCompareNode))]
        [Description("Сравнить данные между собой.")]
        [Path("Логика")]
        [Tags("compare", "comparison", "equals", "сравнение", "chfdytybt")]
        Compare,
        /// <summary>
        /// Выбор данных
        /// </summary>
        [Name("Выбрать данные"), Node(typeof(DialogProjectSelectIfNode))]
        [Description("Выбрать данные в зависимости от входного значения. 0 = false, остальное - true.")]
        [Path("Логика")]
        [Tags("if", "else", "если", "или", "ещё")]
        [Tags("eckb", "bkb", "to`")]
        SelectIf,
        /// <summary>
        /// Ветвление
        /// </summary>
        [Name("Ветвление"), Node(typeof(DialogProjectDoIfNode))]
        [Description("Выполнить следующее действие в зависимости от входного значения. 0 = false, остальное - true.")]
        [Path("Логика")]
        [Tags("if", "else", "если", "или", "ещё", "условие")]
        [Tags("eckb", "bkb", "to`", "eckjdbt")]
        DoIf,

        // Вычисления
        /// <summary>
        /// Сложение данных
        /// </summary>
        [Name("Сложить"), Node(typeof(DialogProjectAddNode))]
        [Description("Сложить данные.")]
        [Path("Вычисления")]
        [Tags("combine", "add", "plus", "добавить", "объединить")]
        [Tags("lj,fdbnm", "j,]tlbybnm")]
        Add,
        /// <summary>
        /// Вычитание данных
        /// </summary>
        [Name("Вычесть"), Node(typeof(DialogProjectSubtractNode))]
        [Description("Вычесть данные.")]
        [Path("Вычисления")]
        [Tags("subtract", "remove", "minus", "убрать", "удалить")]
        [Tags("e,hfnm", "elfkbnm")]
        Subtract,
        /// <summary>
        /// Умножение данных
        /// </summary>
        [Name("Умножить"), Node(typeof(DialogProjectMultiplyNode))]
        [Description("Умножить данные.")]
        [Path("Вычисления")]
        [Tags("multiply", "mlp", "evyj;bnm")]
        Multiply,
        /// <summary>
        /// Деление данных
        /// </summary>
        [Name("Разделить"), Node(typeof(DialogProjectDivideNode))]
        [Description("Разделить данные.")]
        [Path("Вычисления")]
        [Tags("divide", "hfpltkbnm")]
        Divide,
        /// <summary>
        /// Замена значений в строке
        /// </summary>
        [Name("Заменить"), Node(typeof(DialogProjectReplaceNode))]
        [Description("Заменить данные.")]
        [Path("Вычисления")]
        [Tags("replace", "pfvtybnm")]
        Replace,

        // Данные
        /// <summary>
        /// Переменная
        /// </summary>
        [Name("Переменная"), Node(typeof(DialogProjectVariableNode))]
        [Description("Переменная проекта, набора или диалога.")]
        [Path("Данные")]
        [Tags("variable", "gthtvtyyfz")]
        Variable,
        /// <summary>
        /// Ссылка на ресурс
        /// </summary>
        [Name("Ресурс"), Node(typeof(DialogProjectReferenceNode))]
        [Description("Ресурс проекта, набора или диалога.")]
        [Path("Данные")]
        [Tags("resource", "reference", "htcehc")]
        Reference,
        /// <summary>
        /// Строка
        /// </summary>
        [Name("Строка"), Node(typeof(DialogProjectStringNode))]
        [Description("Любая строка.")]
        [Path("Данные")]
        [Tags("string", "cnhjrf")]
        String,
        /// <summary>
        /// Число
        /// </summary>
        [Name("Число"), Node(typeof(DialogProjectNumberNode))]
        [Description("Любое число.")]
        [Path("Данные")]
        [Tags("number", "float", "int", "double", "xbckj")]
        Number,
        /// <summary>
        /// Выбор ответа
        /// </summary>
        [Name("Вариант ответа"), Node(typeof(DialogProjectSimpleChoiceNode))]
        [Description("Выбор ответа.")]
        [Path("Диалог")]
        [Tags("choice", "выбор", "ответы", "ds,jh", "jndtns")]
        SimpleChoice,
        /// <summary>
        /// Случайное число
        /// </summary>
        [Name("Случайное число"), Node(typeof(DialogProjectRandomNode))]
        [Description("Случайное число в указанном диапазоне.")]
        [Path("Данные")]
        [Tags("random", "rnd", "number", "ckexfqyjt", "xbckj")]
        RandomNumber,
        /// <summary>
        /// Событие изменения состояния выполнения диалога
        /// </summary>
        [Name("Состояние диалога"), Node(typeof(DialogProjectEventNode))]
        [Description("Выполнить действия при вызове определённого события диалога.")]
        [Path("Диалог")]
        [Tags("event", "state", "status", "cjcnjzybt", "lbfkjuf")]
        [Tags("статус", "событие", "cnfnec", "cj,snbt")]
        Event,
        /// <summary>
        /// Ожидание
        /// </summary>
        [Name("Ждать"), Node(typeof(DialogProjectWaitNode))]
        [Description("Подождать определённое количество времени.")]
        [Path("Поток")]
        [Tags("await", "delay", "ожидание", "задержка", "pflth;rf", "j;blfybt")]
        Wait,
        /// <summary>
        /// Объединение потоков
        /// </summary>
        [Name("Объединить"), Node(typeof(DialogProjectJoinNode))]
        [Description("Ожидать завершение всех входящих потоков, а затем объединить их в один.")]
        [Path("Поток")]
        [Tags("combine", "join", "j,]tlbybnm")]
        Join,
        /// <summary>
        /// Отсечение потоков
        /// </summary>
        [Name("Только первый вход"), Node(typeof(DialogProjectIntersectNode))]
        [Description("Принять первый поток и продолжить его выполнение, завершая все последующие входящие потоки.")]
        [Path("Поток")]
        [Tags("intersect")]
        Intersect,
        /// <summary>
        /// Минимальное значение
        /// </summary>
        [Name("Минимум"), Node(typeof(DialogProjectMinimumNode))]
        [Description("Выбрать минимальное из двух входящих значений.")]
        [Path("Вычисления")]
        [Tags("minimum", "vbybvev")]
        Minimum,
        /// <summary>
        /// Максимальное значение
        /// </summary>
        [Name("Максимум"), Node(typeof(DialogProjectMaximumNode))]
        [Description("Выбрать максимальное из двух входящих значений.")]
        [Path("Вычисления")]
        [Tags("maximum", "vfrcbvev")]
        Maximum,
        /// <summary>
        /// Эмоция
        /// </summary>
        [Name("Эмоция"), Node(typeof(DialogProjectEmotionNode))]
        [Description("Отобразить эмоцию персонажем.")]
        [Path("Диалог")]
        [Tags("emotion", "'vjwbz")]
        Emotion,
        /// <summary>
        /// Точка входа в диалог
        /// </summary>
        [Name("Точка входа"), Node(typeof(DialogProjectEntryNode))]
        [Description("Точка входа в диалог. Может быть полезно при отсутствии узла, который может быть явной точкой входа")]
        [Path("Диалог")]
        [Tags("start", "entry", "point", "начало")]
        [Tags("njxrf", "d[jlf", "yfxfkj")]
        Entry,
        /// <summary>
        /// Описание
        /// </summary>
        [Name("Описание"), Node(typeof(DialogProjectStickerNode))]
        [Description("Текст для описания чего либо")]
        [Path("Дополнительно")]
        [Tags("sticker", "description", "text", "текст", "описание", "стикер")]
        [Tags("ntrcn", "jgbcfybt", "cnbrth")]
        Sticker,
        /// <summary>
        /// Предустановленное событие
        /// </summary>
        [Name("Событие"), Node(typeof(DialogProjectCustomTriggerNode))]
        [Description("Вызвать событие. Событие будет получено обработчиком диалога.")]
        [InternalNode]
        CustomTrigger,
    }
}

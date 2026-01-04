namespace DialogMaker.Core.Executioning
{
    public enum DialogByteCode : byte
    {
        /// <summary>
        /// Показать реплику персонажа.
        /// 1 аргумент - персонаж, 2 - текст (переменная)
        /// </summary>
        [ArgsCount(2), Implementation(typeof(ShowReplicaOpCode))]
        ShowReplica,
        /// <summary>
        /// Показать реплику персонажа.
        /// 1 аргумент - персонаж, 2 - текст (ресурс)
        /// </summary>
        [ArgsCount(2), Implementation(typeof(ShowResourceReplicaOpCode))]
        ShowResourceReplica,
        /// <summary>
        /// Показать полноэкранную реплику персонажа. 
        /// 1 аргумент - персонаж, 2 - текст (переменная), 3 - фон.
        /// Фоном реплики может быть картинка, видео или цвет
        /// </summary>
        [ArgsCount(3), Implementation(typeof(ShowFullScreenReplicaOpCode))]
        ShowFullScreenReplica,
        /// <summary>
        /// Показать полноэкранную реплику персонажа. 
        /// 1 аргумент - персонаж, 2 - текст (ресурс), 3 - фон.
        /// Фоном реплики может быть картинка, видео или цвет
        /// </summary>
        [ArgsCount(3), Implementation(typeof(ShowResourceFullScreenReplicaOpCode))]
        ShowResourceFullScreenReplica,
        /// <summary>
        /// Показать реплику персонажа на весь экран - текст по центру.
        /// 1 аргумент - персонаж, 2 - текст (переменная), 3 - цвет фона, 4 - цвет текста
        /// </summary>
        [ArgsCount(4), Implementation(typeof(ShowColorReplicaOpCode))]
        ShowColorReplica,
        /// <summary>
        /// Показать реплику персонажа на весь экран - текст по центру.
        /// 1 аргумент - персонаж, 2 - текст (ресурс), 3 - цвет фона, 4 - цвет текста
        /// </summary>
        [ArgsCount(4), Implementation(typeof(ShowResourceColorReplicaOpCode))]
        ShowResourceColorReplica,
        /// <summary>
        /// Дать выбор ответа.
        /// 1 аргумент - персонаж, 2 - варианты ответа, 3 - переменная в которую будет записан ответ
        /// </summary>
        [ArgsCount(3), Implementation(typeof(ShowChoiceOpCode))]
        ShowChoice,
        /// <summary>
        /// Вызывать пользовательский триггер.
        /// 1 аргумент - идентификатор триггера
        /// </summary>
        [ArgsCount(1), Implementation(typeof(TriggerOpCode))]
        Trigger,

        /// <summary>
        /// Равны ли значения?
        /// 1 аргумент - первая переменная, 2 - вторая переменная
        /// </summary>
        /// <remarks>Результат помещается в стек.</remarks>
        [ArgsCount(2), Implementation(typeof(EqualsOpCode))]
        Equals,
        /// <summary>
        /// Неравны ли значения?
        /// 1 аргумент - первая переменная, 2 - вторая переменная
        /// </summary>
        /// <remarks>Результат помещается в стек.</remarks>
        [ArgsCount(2), Implementation(typeof(NotEqualsOpCode))]
        NotEquals,
        /// <summary>
        /// Первое значение больше второго?
        /// 1 аргумент - первая переменная, 2 - вторая переменная
        /// </summary>
        /// <remarks>Результат помещается в стек.</remarks>
        [ArgsCount(2), Implementation(typeof(AboveOpCode))]
        Above,
        /// <summary>
        /// Первое значение больше или равно второму?
        /// 1 аргумент - первая переменная, 2 - вторая переменная
        /// </summary>
        /// <remarks>Результат помещается в стек.</remarks>
        [ArgsCount(2), Implementation(typeof(AboveOrEqualsOpCode))]
        AboveOrEquals,
        /// <summary>
        /// Первое значение меньше второго?
        /// 1 аргумент - первая переменная, 2 - вторая переменная
        /// </summary>
        /// <remarks>Результат помещается в стек.</remarks>
        [ArgsCount(2), Implementation(typeof(LessOpCode))]
        Less,
        /// <summary>
        /// Первое значение меньше или равно второму?
        /// 1 аргумент - первая переменная, 2 - вторая переменная
        /// </summary>
        /// <remarks>Результат помещается в стек.</remarks>
        [ArgsCount(2), Implementation(typeof(LessOrEqualsOpCode))]
        LessOrEquals,

        /// <summary>
        /// Задать первой переменной значение второй.
        /// 1 аргумент - первая переменная, 2 - вторая переменная
        /// </summary>
        [ArgsCount(2), Implementation(typeof(SetOpCode))]
        Set,
        /// <summary>
        /// Задать переменной последнее значение из стека.
        /// 1 аргумент - переменная
        /// </summary>
        [ArgsCount(1), Implementation(typeof(StackToVariableOpCode))]
        StackToVariable,
        /// <summary>
        /// Добавить к первой переменной значение второй.
        /// 1 аргумент - первая переменная, 2 - вторая переменная
        /// </summary>
        [ArgsCount(2), Implementation(typeof(AddOpCode))]
        Add,
        /// <summary>
        /// Вычесть из первой переменной значение второй.
        /// 1 аргумент - первая переменная, 2 - вторая переменная
        /// </summary>
        [ArgsCount(2), Implementation(typeof(SubtractOpCode))]
        Subtract,
        /// <summary>
        /// Умножить первую переменную на значение второй.
        /// 1 аргумент - первая переменная, 2 - вторая переменная
        /// </summary>
        [ArgsCount(2), Implementation(typeof(MultiplyOpCode))]
        Multiply,
        /// <summary>
        /// Разделить первую переменную на значение второй.
        /// 1 аргумент - первая переменная, 2 - вторая переменная
        /// </summary>
        [ArgsCount(2), Implementation(typeof(DivideOpCode))]
        Divide,
        /// <summary>
        /// Заменить в первой переменной одно значение на другое.
        /// 1 аргумент - переменная, 2 - искомое значение, 3 - новое значение
        /// </summary>
        [ArgsCount(3), Implementation(typeof(ReplaceOpCode))]
        Replace,
        /// <summary>
        /// Получить случайное число в определённом диапазоне.
        /// 1 аргумент - минимальное значение, 2 - максимальное значение, 
        /// 3 - целое число, 4 - переменная в которую будет записано число
        /// </summary>
        [ArgsCount(4), Implementation(typeof(RandomNumberOpCode))]
        RandomNumber,

        /// <summary>
        /// Начать немедленное выполнение секции.
        /// Аргумент - индекс секции
        /// </summary>
        [ArgsCount(1), Implementation(typeof(JumpOpCode))]
        Jump,
        /// <summary>
        /// Начать немедленное выполнение секции, если последнее значение в стеке - true.
        /// Аргумент - индекс секции
        /// </summary>
        [ArgsCount(1), Implementation(typeof(JumpIfTrueOpCode))]
        JumpIfTrue,
        /// <summary>
        /// Завершить диалог
        /// </summary>
        [ArgsCount(0), Implementation(typeof(EndOpCode))]
        End,
        /// <summary>
        /// Начать поток. 
        /// Аргумент - индекс секции с которой начнётся новый поток
        /// </summary>
        [ArgsCount(1), Implementation(typeof(StartThreadOpCode))]
        StartThread,
        /// <summary>
        /// Завершить поток
        /// </summary>
        [ArgsCount(0), Implementation(typeof(EndThreadOpCode))]
        EndThread,
        /// <summary>
        /// Подождать.
        /// 1 аргумент - время ожидания в секундах
        /// </summary>
        [ArgsCount(1), Implementation(typeof(WaitOpCode))]
        Wait,
        /// <summary>
        /// Перейти к выполнению инструкции.
        /// 1 аргумент - позиция инструкции
        /// </summary>
        [ArgsCount(1), Implementation(typeof(GotoOpCode))]
        Goto,
        /// <summary>
        /// Перейти к выполнению инструкции, если последнее значение в стеке - true.
        /// 1 аргумент - позиция инструкции
        /// </summary>
        [ArgsCount(1), Implementation(typeof(GotoIfTrueOpCode))]
        GotoIfTrue,
        /// <summary>
        /// Пустая инструкция, которая ничего не делает
        /// </summary>
        [ArgsCount(0), Implementation(typeof(EmptyOpCode))]
        Empty
    }
}

namespace DialogMaker.Core.Scripting.Tokens
{
    /// <summary>
    /// Тип токена скрипта диалога
    /// </summary>
    public enum DialogScriptTokenType
    {
        /// <summary>
        /// Неизвестный токен
        /// </summary>
        Unknown,
        /// <summary>
        /// Объявление переменной и/или присвоение ей значения
        /// </summary>
        [Parser(typeof(DialogScriptVariableTokenParser))]
        Variable,
        /// <summary>
        /// Объявление функции
        /// </summary>
        Function,
        /// <summary>
        /// if блок
        /// </summary>
        IfBlock,
        /// <summary>
        /// Блок цикла for
        /// </summary>
        ForBlock,
        /// <summary>
        /// Блок цикла while
        /// </summary>
        WhileBlock,
        /// <summary>
        /// Прерывание цикла
        /// </summary>
        [Parser(typeof(DialogScriptBreakTokenParser))]
        Break,
        /// <summary>
        /// Пропуск итерации цикла
        /// </summary>
        [Parser(typeof(DialogScriptContinueTokenParser))]
        Continue,
        /// <summary>
        /// Возвращение значение или завершение выполнения функции
        /// </summary>
        Return,
        /// <summary>
        /// Атрибут
        /// </summary>
        [Parser(typeof(DialogScriptVariableTokenParser))]
        Attribute,
        /// <summary>
        /// Условие (==, !=, >, >=, <, <=)
        /// </summary>
        Statement,
        /// <summary>
        /// Математическая операция (+, -, +=, -=, *, /, *=, /=)
        /// </summary>
        Math,
        /// <summary>
        /// Увеличение значения переменной на 1
        /// </summary>
        Increment,
        /// <summary>
        /// Уменьшение значения переменной на 1
        /// </summary>
        Decrement,
        /// <summary>
        /// Вызов функции
        /// </summary>
        Call,
        /// <summary>
        /// Число
        /// </summary>
        [Parser(typeof(DialogScriptNumberTokenParser))]
        Number,
        /// <summary>
        /// Булево значение
        /// </summary>
        [Parser(typeof(DialogScriptBoolTokenParser))]
        Bool,
        /// <summary>
        /// Строка
        /// </summary>
        [Parser(typeof(DialogScriptStringTokenParser))]
        String,
        /// <summary>
        /// Пустое значение
        /// </summary>
        [Parser(typeof(DialogScriptNullTokenParser))]
        Null,
        /// <summary>
        /// switch блок
        /// </summary>
        SwitchBlock,
        /// <summary>
        /// Случай значения switch блока
        /// </summary>
        Case,
        /// <summary>
        /// Условие по умолчанию switch блока
        /// </summary>
        Default,
    }
}

using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;

namespace DialogMaker.Core
{
    /// <summary>
    /// Разрешённые типы значений
    /// </summary>
    [Flags]
    public enum AllowedObjectValues
    {
        /// <summary>
        /// Числа
        /// </summary>
        [Name("Число"), Type(typeof(float), 0f), Type(typeof(double), 0d), Type(typeof(int), 0)]
        [PortType(DialogNodePortType.Number)]
        Number = 1 << 1,
        /// <summary>
        /// Строки
        /// </summary>
        [Name("Строка"), Type(typeof(string), "")]
        [PortType(DialogNodePortType.String)]
        String = 1 << 2,
        /// <summary>
        /// Булевы значения
        /// </summary>
        [Name("Переключатель"), Type(typeof(bool), false)]
        [PortType(DialogNodePortType.Bool)]
        Bool = 1 << 3,
        /// <summary>
        /// Ресурсы
        /// </summary>
        [Name("Ресурс"), Type(typeof(DialogProjectReference), null!)]
        [PortType(DialogNodePortType.Object)]
        Resource = 1 << 4,
        /// <summary>
        /// Списки
        /// </summary>
        [Name("Список"), Type(typeof(EditableCollection<object>), null!)]
        [PortType(DialogNodePortType.Object)]
        List = 1 << 5,
        /// <summary>
        /// Все типы, кроме списков
        /// </summary>
        AllWithoutList = Number | String | Bool | Resource,
        /// <summary>
        /// Стандартные типы (числа, строки, булевы значения)
        /// </summary>
        Native = Number | String | Bool,
        /// <summary>
        /// Все типы
        /// </summary>
        All = AllWithoutList | List
    }
}

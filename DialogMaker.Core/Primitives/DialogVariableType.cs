using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;

namespace DialogMaker.Core
{
    /// <summary>
    /// Тип переменной диалога
    /// </summary>
    public enum DialogVariableType : byte
    {
        /// <summary>
        /// Числовой тип (<see cref="float"/>, <see cref="int"/>, <see cref="double"/>)
        /// </summary>
        [Name("Число"), Type(typeof(DialogProjectVariableNumber), 0)]
        [Types(typeof(float), typeof(double), typeof(int))]
        Number = DialogNodePortType.Number,
        /// <summary>
        /// Булевый тип (<see cref="bool"/>)
        /// </summary>
        [Name("Переключатель"), Type(typeof(DialogProjectVariableBool), false)]
        [Types(typeof(bool))]
        Bool = DialogNodePortType.Bool,
        /// <summary>
        /// Строковой тип
        /// </summary>
        [Name("Строка"), Type(typeof(DialogProjectVariableString), "")]
        [Types(typeof(string))]
        String = DialogNodePortType.String
    }
}

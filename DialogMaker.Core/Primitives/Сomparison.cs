using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Core.Executioning;
using System.ComponentModel;

namespace DialogMaker.Core
{
    /// <summary>
    /// Тип сравнения
    /// </summary>
    public enum Comparison
    {
        /// <summary>
        /// Равенство
        /// </summary>
        [Name("Равно"), Description("==")]
        Equals = DialogByteCode.Equals,
        /// <summary>
        /// Неравенство
        /// </summary>
        [Name("Не равно"), Description("!=")]
        NotEquals = DialogByteCode.NotEquals,
        /// <summary>
        /// Больше
        /// </summary>
        [Name("Больше"), Description(">")]
        Greater = DialogByteCode.Above,
        /// <summary>
        /// Больше или равно
        /// </summary>
        [Name("Больше или равно"), Description(">=")]
        GreaterOrEquals = DialogByteCode.AboveOrEquals,
        /// <summary>
        /// Меньше
        /// </summary>
        [Name("Меньше"), Description("<")]
        Less = DialogByteCode.Less,
        /// <summary>
        /// Меньше или равно
        /// </summary>
        [Name("Меньше или равно"), Description("<=")]
        LessOrEquals = DialogByteCode.LessOrEquals,
    }
}

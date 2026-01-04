using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Core.Executioning;

namespace DialogMaker.Core
{
    public enum Comparison
    {
        [Name("Равно")]
        Equals = DialogByteCode.Equals,
        [Name("Не равно")]
        NotEquals = DialogByteCode.NotEquals,
        [Name("Больше")]
        Greater = DialogByteCode.Above,
        [Name("Больше или равно")]
        GreaterOrEquals = DialogByteCode.AboveOrEquals,
        [Name("Меньше")]
        Less = DialogByteCode.Less,
        [Name("Меньше или равно")]
        LessOrEquals = DialogByteCode.LessOrEquals,
    }
}

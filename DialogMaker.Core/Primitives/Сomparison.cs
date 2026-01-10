using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Core.Executioning;
using System.ComponentModel;

namespace DialogMaker.Core
{
    public enum Comparison
    {
        [Name("Равно"), Description("==")]
        Equals = DialogByteCode.Equals,
        [Name("Не равно"), Description("!=")]
        NotEquals = DialogByteCode.NotEquals,
        [Name("Больше"), Description(">")]
        Greater = DialogByteCode.Above,
        [Name("Больше или равно"), Description(">=")]
        GreaterOrEquals = DialogByteCode.AboveOrEquals,
        [Name("Меньше"), Description("<")]
        Less = DialogByteCode.Less,
        [Name("Меньше или равно"), Description("<=")]
        LessOrEquals = DialogByteCode.LessOrEquals,
    }
}

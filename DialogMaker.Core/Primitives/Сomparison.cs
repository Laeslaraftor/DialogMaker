using DialogMaker.Core.Editor.Nodes;

namespace DialogMaker.Core
{
    public enum Comparison
    {
        [Name("Равно")]
        Equals,
        [Name("Не равно")]
        NotEquals,
        [Name("Больше")]
        Greater,
        [Name("Больше или равно")]
        GreaterOrEquals,
        [Name("Меньше")]
        Less,
        [Name("Меньше или равно")]
        LessOrEquals
    }
}

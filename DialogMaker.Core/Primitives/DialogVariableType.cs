using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;

namespace DialogMaker.Core
{
    public enum DialogVariableType : byte
    {
        [Name("Число"), Type(typeof(DialogProjectVariableNumber), 0)]
        [Types(typeof(float), typeof(double), typeof(int))]
        Number = DialogNodePortType.Number,
        [Name("Переключатель"), Type(typeof(DialogProjectVariableBool), 0)]
        [Types(typeof(bool))]
        Bool = DialogNodePortType.Bool,
        [Name("Строка"), Type(typeof(DialogProjectVariableString), 0)]
        [Types(typeof(string))]
        String = DialogNodePortType.String
    }
}

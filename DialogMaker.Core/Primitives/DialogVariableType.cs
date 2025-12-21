using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;

namespace DialogMaker.Core
{
    public enum DialogVariableType : byte
    {
        [Name("Число"), Type(typeof(DialogProjectVariableNumber), 0)]
        Number = DialogNodePortType.Number,
        [Name("Переключатель"), Type(typeof(DialogProjectVariableBool), 0)]
        Bool = DialogNodePortType.Bool,
        [Name("Строка"), Type(typeof(DialogProjectVariableString), 0)]
        String = DialogNodePortType.String
    }
}

using Acly.Execution;
using DialogMaker.Core.Editor.Nodes;
using System.Drawing;

namespace DialogMaker.Core
{
    public enum DialogNodePortType : byte
    {
        [Name("Переключатель"), Color(128, 128, 128), Type(typeof(bool), false)]
        Bool = CodeDataType.Bool,
        [Name("Число"), Color(128, 128, 128), Type(typeof(float), 0f), Type(typeof(int), 0)]
        Number = CodeDataType.Float,
        [Name("Строка"), Color(60, 198, 176), Type(typeof(string), "")]
        String = CodeDataType.String,
        [Name("Объект"), Color(77, 107, 254)]
        [Type(typeof(object), null!), Type(typeof(int), 0f), Type(typeof(bool), false), Type(typeof(float), 0f), Type(typeof(string), "")]
        Object,
        [Name("Действие"), Color(245, 73, 39)]
        Action = byte.MaxValue
    }
}

using Acly.Execution;
using DialogMaker.Core.Editor.Nodes;
using System.Drawing;

namespace DialogMaker.Core
{
    public enum DialogNodePortType : byte
    {
        [Color(128, 128, 128), Type(typeof(bool), false)]
        Bool = CodeDataType.Bool,
        [Color(128, 128, 128), Type(typeof(float), 0f), Type(typeof(int), 0)]
        Number = CodeDataType.Float,
        [Color(60, 198, 176), Type(typeof(string), "")]
        String = CodeDataType.String,
        [Color(77, 107, 254)]
        [Type(typeof(int), 0f), Type(typeof(bool), false), Type(typeof(float), 0f), Type(typeof(string), "")]
        Object,
        [Color(245, 73, 39)]
        Action = byte.MaxValue
    }
}

using Acly.Execution;
using DialogMaker.Core.Editor.Nodes;
using System.Drawing;

namespace DialogMaker.Core
{
    public enum DialogNodePortType : byte
    {
        [Color(128, 128, 128)]
        Bool = CodeDataType.Bool,
        [Color(128, 128, 128)]
        Integer = CodeDataType.Int32,
        [Color(128, 128, 128)]
        Float = CodeDataType.Float,
        [Color(60, 198, 176)]
        String = CodeDataType.String,
        [Color(77, 107, 254)]
        Object,
        [Color(245, 73, 39)]
        Action = byte.MaxValue
    }
}

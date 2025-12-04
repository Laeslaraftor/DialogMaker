using Acly.Execution;

namespace DialogMaker.Core
{
    public enum DialogNodePortType : byte
    {
        Bool = CodeDataType.Bool,
        Integer = CodeDataType.Int32,
        Float = CodeDataType.Float,
        String = CodeDataType.String,
        Action = byte.MaxValue
    }
}

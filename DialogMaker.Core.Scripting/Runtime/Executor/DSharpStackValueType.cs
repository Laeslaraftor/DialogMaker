namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Type of value that stores in stack
    /// </summary>
    public enum DSharpStackValueType : byte
    {
        Null,
        Byte,
        SByte,
        Short,
        UShort,
        Int,
        UInt,
        Long,
        ULong,
        Decimal,
        Double,
        Float,
        Char,
        Bool,
        Structure,
        Reference
    }
}

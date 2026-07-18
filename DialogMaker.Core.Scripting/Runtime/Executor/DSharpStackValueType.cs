namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Type of value that stores in stack
    /// </summary>
    public enum DSharpStackValueType : byte
    {
        Null,
        [LiteralType(DSharpLiteralType.Byte)]
        Byte,
        [LiteralType(DSharpLiteralType.SByte)]
        SByte,
        [LiteralType(DSharpLiteralType.Short)]
        Short,
        [LiteralType(DSharpLiteralType.UShort)]
        UShort,
        [LiteralType(DSharpLiteralType.Int)]
        Int,
        [LiteralType(DSharpLiteralType.UInt)]
        UInt,
        [LiteralType(DSharpLiteralType.Long)]
        Long,
        [LiteralType(DSharpLiteralType.ULong)]
        ULong,
        [LiteralType(DSharpLiteralType.Decimal)]
        Decimal,
        [LiteralType(DSharpLiteralType.Double)]
        Double,
        [LiteralType(DSharpLiteralType.Float)]
        Float,
        [LiteralType(DSharpLiteralType.Char)]
        Char,
        [LiteralType(DSharpLiteralType.Bool)]
        Bool,
        [LiteralType(DSharpLiteralType.NInt)]
        Nint,
        [LiteralType(DSharpLiteralType.NUInt)]
        Nuint,
        Structure,
        Reference,
        MethodCallingInfo,
        MethodParametersBuffer,
        Scope
    }
}

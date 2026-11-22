using DialogMaker.Core.Scripting.CodeAnalyzer;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Literal type
    /// </summary>
    [GenerateInformation(EnumValuesInformation.OnlyValues)]
    public enum DSharpLiteralType : byte
    {
        /// <summary>
        /// Null value
        /// </summary>
        Null,
        /// <summary>
        /// Boolean value
        /// </summary>
        Bool,
        /// <summary>
        /// 8-bit unsigned number
        /// </summary>
        Byte,
        /// <summary>
        /// 8-bit signed number
        /// </summary>
        SByte,
        /// <summary>
        /// Char value
        /// </summary>
        Char,
        /// <summary>
        /// 16-bit signed number
        /// </summary>
        Short,
        /// <summary>
        /// 16-bit unsigned number
        /// </summary>
        UShort,
        /// <summary>
        /// 32-bit signed number
        /// </summary>
        Int,
        /// <summary>
        /// 32-bit unsigned number
        /// </summary>
        UInt,
        /// <summary>
        /// 64-bit signed number
        /// </summary>
        Long,
        /// <summary>
        /// 64-bit unsigned number
        /// </summary>
        ULong,
        /// <summary>
        /// Native signed integer (pointer)
        /// </summary>
        NInt,
        /// <summary>
        /// Native unsigned integer (pointer)
        /// </summary>
        NUInt,
        /// <summary>
        /// Single precision floating-point number
        /// </summary>
        Float,
        /// <summary>
        /// Double precision floating-point number 
        /// </summary>
        Double,
        /// <summary>
        /// Decimal
        /// </summary>
        Decimal,
        /// <summary>
        /// String value
        /// </summary>
        String
    }
}

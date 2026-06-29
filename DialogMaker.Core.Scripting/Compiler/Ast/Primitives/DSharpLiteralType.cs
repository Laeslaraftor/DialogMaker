using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    /// <summary>
    /// Literal type
    /// </summary>
    public enum DSharpLiteralType
    {
        /// <summary>
        /// Null value
        /// </summary>
        Null = DSharpTokenType.Null,
        /// <summary>
        /// 32-bit signed number
        /// </summary>
        Int = DSharpTokenType.Int,
        /// <summary>
        /// 32-bit unsigned number
        /// </summary>
        UInt = DSharpTokenType.UInt,
        /// <summary>
        /// 64-bit signed number
        /// </summary>
        Long = DSharpTokenType.Long,
        /// <summary>
        /// 64-bit unsigned number
        /// </summary>
        ULong = DSharpTokenType.ULong,
        /// <summary>
        /// 16-bit signed number
        /// </summary>
        Short = DSharpTokenType.Short,
        /// <summary>
        /// 16-bit unsigned number
        /// </summary>
        UShort = DSharpTokenType.UShort,
        /// <summary>
        /// 8-bit unsigned number
        /// </summary>
        Byte = DSharpTokenType.Byte,
        /// <summary>
        /// 8-bit signed number
        /// </summary>
        SByte = DSharpTokenType.SByte,
        /// <summary>
        /// Decimal
        /// </summary>
        Decimal = DSharpTokenType.Decimal,
        /// <summary>
        /// Double precision floating-point number 
        /// </summary>
        Double = DSharpTokenType.Double,
        /// <summary>
        /// Single precision floating-point number
        /// </summary>
        Float = DSharpTokenType.Float,
        /// <summary>
        /// Native signed integer (pointer)
        /// </summary>
        NInt = DSharpTokenType.Nint,
        /// <summary>
        /// Native unsigned integer (pointer)
        /// </summary>
        NUInt = DSharpTokenType.Nuint,
        /// <summary>
        /// Boolean value
        /// </summary>
        Bool = DSharpTokenType.Bool,
        /// <summary>
        /// String value
        /// </summary>
        String = DSharpTokenType.String,
        /// <summary>
        /// Char value
        /// </summary>
        Char = DSharpTokenType.Char
    }
}

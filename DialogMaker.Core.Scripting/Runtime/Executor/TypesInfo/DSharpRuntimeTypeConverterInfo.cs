using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo
{
    /// <summary>
    /// Information about converter for D# runtime type
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpRuntimeTypeConverterInfo
    {
        /// <summary>
        /// Is here no converter
        /// </summary>
        public readonly bool IsNull => Converter == null && ConvertingType != DSharpLiteralType.Null;
        /// <summary>
        /// Is number converter
        /// </summary>
        public readonly bool IsNumber => ConvertingType != DSharpLiteralType.String && !IsNull;

        /// <summary>
        /// Type that converting
        /// </summary>
        public DSharpLiteralType ConvertingType;
        /// <summary>
        /// Pointer to converter method
        /// </summary>
        public delegate*<DSharpObject*, DSharpNumber> Converter;
    }
}

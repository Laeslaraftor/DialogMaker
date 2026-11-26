using DialogMaker.Core.Scripting.CodeAnalyzer;
using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Unary operator
    /// </summary>
    [GenerateInformation(EnumValuesInformation.OnlyValues)]
    public enum DSharpUnaryOperator
    {
        /// <summary>
        /// Logical not operator (!)
        /// </summary>
        Not = DSharpTokenType.Not,
        /// <summary>
        /// Minus operator. It multiplies value by -1
        /// </summary>
        [Name("Invert")]
        Minus = DSharpTokenType.Minus,
        /// <summary>
        /// Increment operator (++). It add 1 to value 
        /// </summary>
        Increment = DSharpTokenType.Increment,
        /// <summary>
        /// Decrement operator (--). It subtract 1 from value 
        /// </summary>
        Decrement = DSharpTokenType.Decrement,
        /// <summary>
        /// Bit invert operator (~). It inverts all bits in number 
        /// </summary>
        BitInvert = DSharpTokenType.Tilde,
        /// <summary>
        /// Operator for getting reference to variable or field (<![CDATA[&someVariable]]>)
        /// </summary>
        [Ignore]
        AddressOf = DSharpTokenType.And,
        /// <summary>
        /// Operator for accessing to value through address (<c>*pointer</c> or <c>*pointer = value</c>)
        /// </summary>
        [Ignore]
        Dereference = DSharpTokenType.Multiply,
    }
}

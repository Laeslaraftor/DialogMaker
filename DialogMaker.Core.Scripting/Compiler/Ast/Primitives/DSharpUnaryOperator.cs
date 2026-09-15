using DialogMaker.Core.Scripting.CodeAnalyzer;
using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast
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
        /// Operator for getting reference to variable or field (<![CDATA[&someVariable]]>)
        /// </summary>
        AddressOf = DSharpTokenType.Multiply,
        /// <summary>
        /// Operator for accessing to value through address (<c>*pointer</c> or <c>*pointer = value</c>)
        /// </summary>
        Dereference = DSharpTokenType.And,
    }
}

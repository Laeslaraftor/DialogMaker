using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler
{
    /// <summary>
    /// D# abstract syntax tree exception
    /// </summary>
    public class DSharpAstException : DSharpException
    {
        public DSharpAstException(string message, DSharpToken token) 
            : base(message)
        {
            Token = token;
        }
        public DSharpAstException(string message, DSharpToken token, Exception? innerException) 
            : base(message, innerException)
        {
            Token = token;
        }

        /// <summary>
        /// Token on that exceptions was thrown
        /// </summary>
        public DSharpToken Token { get; }
        public override string Message => $"{base.Message} {Token.ToPositionString()}";
    }
}

using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Statement that represents unsafe block
    /// </summary>
    /// <param name="token">Token that represents unsafe keyword</param>
    public partial class UnsafeStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Block body
        /// </summary>
        public BlockStatementNode? Body { get; set; }

        #region Static

        /// <summary>
        /// Parse unsafe statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed unsafe statement</returns>
        public static UnsafeStatementNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DSharpTokenType.Unsafe);

            return new(token)
            { 
                Body = BlockStatementNode.Parse(stream, DSharpStatementType.Code)
            };
        }

        #endregion
    }
}

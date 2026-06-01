using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Using statement
    /// </summary>
    /// <param name="token">Token that represents using keyword</param>
    public class UsingStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Identifier of namespace
        /// </summary>
        public IdentifierExpressionNode? Identifier { get; set; }

        #region Статика

        /// <summary>
        /// Parse using statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed using statement</returns>
        public static UsingStatementNode Parse(AstParserStream stream)
        {
            var usingToken = stream.Eat(DSharpTokenType.Using);
            var identifier = IdentifierExpressionNode.Parse(stream, false);
            stream.Eat(DSharpTokenType.Semicolon);

            return new(usingToken)
            {
                Identifier = identifier
            };
        }

        #endregion
    }
}

using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents goto operation
    /// </summary>
    /// <param name="token">Token that represents goto keyword</param>
    public partial class GotoStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Expression of jump destination.
        /// This can be literal value or identifier (label name)
        /// </summary>
        public ExpressionNode? DestinationExpression { get; set; }

        #region Static

        /// <summary>
        /// Parse goto statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed goto statement</returns>
        public static GotoStatementNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DSharpTokenType.Goto);
            GotoStatementNode gotoStatement = new(token)
            {
                DestinationExpression = ExpressionNode.ParseExpression(stream)
            };
            stream.Eat(DSharpTokenType.Semicolon);
            gotoStatement.DestinationExpression.Parent = gotoStatement;

            return gotoStatement;
        }

        #endregion
    }
}

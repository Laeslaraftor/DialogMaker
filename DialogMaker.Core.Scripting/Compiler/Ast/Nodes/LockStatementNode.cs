using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents lock statement
    /// </summary>
    /// <param name="token">Token that represents lock token</param>
    public partial class LockStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Expression for getting value for locking
        /// </summary>
        public ExpressionNode? ValueExpression { get; set; }
        /// <summary>
        /// Body of lock statement
        /// </summary>
        public BlockStatementNode? Body { get; set; }

        #region Static

        /// <summary>
        /// Parse lock statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed lock statement</returns>
        public static LockStatementNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DSharpTokenType.Lock);
            LockStatementNode result = new(token);

            stream.Eat(DSharpTokenType.LeftParen);
            result.ValueExpression = ExpressionNode.ParseExpression(stream);
            stream.Eat(DSharpTokenType.RightParen);
            result.Body = BlockStatementNode.Parse(stream, DSharpStatementType.Code);

            result.ValueExpression.Parent = result;
            result.Body.Parent = result;

            return result;
        }

        #endregion
    }
}

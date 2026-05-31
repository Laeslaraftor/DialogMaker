using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// While statement node
    /// </summary>
    /// <param name="token">Token that represents while keyword</param>
    public class WhileStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Condition for execution body
        /// </summary>
        public ExpressionNode? Condition { get; set; }
        /// <summary>
        /// Body that executes while condition is true
        /// </summary>
        public StatementNode? Body { get; set; }

        #region Статика

        /// <summary>
        /// Parse while statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed while statement</returns>
        public static WhileStatementNode Parse(AstParserStream stream)
        {
            var whileToken = stream.Eat(DSharpTokenType.While);
            stream.Eat(DSharpTokenType.LeftParen);

            WhileStatementNode statement = new(whileToken)
            {
                Condition = ExpressionNode.ParseExpression(stream)
            };

            stream.Eat(DSharpTokenType.RightParen);
            
            if (!stream.Check(DSharpTokenType.Semicolon))
            {
                statement.Body = ParseStatement(stream);
            }
            else
            {
                stream.Eat(DSharpTokenType.Semicolon);
            }

            return statement;
        }

        #endregion
    }
}

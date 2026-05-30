using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// While statement node
    /// </summary>
    /// <param name="token">Token that represents while keyword</param>
    public class WhileStatementNode(DialogScriptToken token) : StatementNode(token)
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
            var whileToken = stream.Eat(DialogScriptTokenType.While);
            stream.Eat(DialogScriptTokenType.LeftParen);

            WhileStatementNode statement = new(whileToken)
            {
                Condition = ExpressionNode.ParseExpression(stream)
            };

            stream.Eat(DialogScriptTokenType.RightParen);
            
            if (!stream.Check(DialogScriptTokenType.Semicolon))
            {
                statement.Body = ParseStatement(stream);
            }
            else
            {
                stream.Eat(DialogScriptTokenType.Semicolon);
            }

            return statement;
        }

        #endregion
    }
}

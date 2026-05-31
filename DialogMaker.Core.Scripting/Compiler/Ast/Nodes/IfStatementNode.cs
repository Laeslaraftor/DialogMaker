using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// If statement node
    /// </summary>
    /// <param name="token">Token that represents if keyword</param>
    public class IfStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Condition for execution "then" branch
        /// </summary>
        public ExpressionNode? Condition { get; set; }
        /// <summary>
        /// Branch that executes when condition is true
        /// </summary>
        public StatementNode? ThenBranch { get; set; }
        /// <summary>
        /// Branch that executes then condition is false
        /// </summary>
        public StatementNode? ElseBranch { get; set; }

        #region Статика

        /// <summary>
        /// Parse if statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed if statement</returns>
        public static IfStatementNode Parse(AstParserStream stream)
        {
            var ifToken = stream.Eat(DSharpTokenType.If);
            stream.Eat(DSharpTokenType.LeftParen);

            IfStatementNode statement = new(ifToken)
            {
                Condition = ExpressionNode.ParseExpression(stream)
            };

            stream.Eat(DSharpTokenType.RightParen);

            statement.ThenBranch = ParseStatement(stream);

            if (stream.Check(DSharpTokenType.Else))
            {
                stream.Eat(DSharpTokenType.Else);
                statement.ElseBranch = ParseStatement(stream);
            }

            return statement;
        }

        #endregion
    }
}

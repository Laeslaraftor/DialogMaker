using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// For statement node
    /// </summary>
    /// <param name="token">Token that represents for keyword</param>
    public class ForStatementNode(DialogScriptToken token) : StatementNode(token)
    {
        /// <summary>
        /// Initializer for conditional and incremental variable
        /// </summary>
        public StatementNode? Initializer { get; set; }
        /// <summary>
        /// Condition for execution body
        /// </summary>
        public ExpressionNode? Condition { get; set; }
        /// <summary>
        /// Increment for initialized variable
        /// </summary>
        public ExpressionNode? Increment { get; set; }
        /// <summary>
        /// Body that executes while condition is true
        /// </summary>
        public StatementNode? Body { get; set; }

        #region Статика

        /// <summary>
        /// Parse for statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed for statement</returns>
        public static ForStatementNode Parse(AstParserStream stream)
        {
            var forToken = stream.Eat(DialogScriptTokenType.For);
            stream.Eat(DialogScriptTokenType.LeftParen);

            ForStatementNode statement = new(forToken)
            {
                Initializer = ParseStatement(stream)
            };

            //stream.Eat(DialogScriptTokenType.Semicolon);

            statement.Condition = ExpressionNode.ParseExpression(stream);
            stream.Eat(DialogScriptTokenType.Semicolon);
            statement.Increment = ExpressionNode.ParseExpression(stream);
            stream.Eat(DialogScriptTokenType.RightParen);
            statement.Body = ParseStatement(stream);

            return statement;
        }

        #endregion
    }
}

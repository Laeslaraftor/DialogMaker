using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// For statement node
    /// </summary>
    /// <param name="token">Token that represents for keyword</param>
    public partial class ForStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Initializer for conditional and incremental variable.
        /// It can be empty
        /// </summary>
        public StatementNode? Initializer { get; set; }
        /// <summary>
        /// Condition for execution body
        /// </summary>
        public ExpressionNode? Condition { get; set; }
        /// <summary>
        /// Increment for initialized variable.
        /// It can be empty
        /// </summary>
        public ExpressionNode? Increment { get; set; }
        /// <summary>
        /// Body that executes while condition is true
        /// </summary>
        public StatementNode? Body { get; set; }

        #region Static

        /// <summary>
        /// Parse for statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed for statement</returns>
        public static ForStatementNode Parse(AstParserStream stream)
        {
            var forToken = stream.Eat(DSharpTokenType.For);
            stream.Eat(DSharpTokenType.LeftParen);

            ForStatementNode forStatement = new(forToken); ;

            if (!stream.Check(DSharpTokenType.Semicolon))
            {
                forStatement.Initializer = ParseCode(stream);
                forStatement.Initializer.Parent = forStatement;
            }

            forStatement.Condition = ExpressionNode.ParseExpression(stream);
            stream.Eat(DSharpTokenType.Semicolon);

            if (!stream.Check(DSharpTokenType.RightParen))
            {
                forStatement.Increment = ExpressionNode.ParseExpression(stream);
                forStatement.Increment.Parent = forStatement;
            }

            stream.Eat(DSharpTokenType.RightParen);
            forStatement.Body = ParseCode(stream);

            forStatement.Condition.Parent = forStatement;
            forStatement.Body.Parent = forStatement;

            return forStatement;
        }

        #endregion
    }
}

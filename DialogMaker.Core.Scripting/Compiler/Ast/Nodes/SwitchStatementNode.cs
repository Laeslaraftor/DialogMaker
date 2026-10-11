using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents switch statement 
    /// </summary>
    /// <param name="token">Token that represents switch keyword</param>
    public partial class SwitchStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Expression of comparing value
        /// </summary>
        public ExpressionNode? ValueExpression { get; set; }
        /// <summary>
        /// List of cases
        /// </summary>
        public List<CaseStatementNode> Cases { get; set; } = [];

        #region Static

        /// <summary>
        /// Parse switch statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed switch statement</returns>
        public static SwitchStatementNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DSharpTokenType.Switch);
            stream.Eat(DSharpTokenType.LeftParen);

            SwitchStatementNode switchStatement = new(token)
            {
                ValueExpression = ExpressionNode.ParseExpression(stream)
            };

            stream.Eat(DSharpTokenType.RightParen);
            stream.Eat(DSharpTokenType.LeftBrace);

            while (!stream.Check(DSharpTokenType.RightBrace))
            {
                var caseStatement = CaseStatementNode.Parse(stream, false);
                caseStatement.Parent = switchStatement;

                switchStatement.Cases.Add(caseStatement);
            }

            stream.Eat(DSharpTokenType.RightBrace);

            switchStatement.ValueExpression.Parent = switchStatement;

            return switchStatement;
        }

        #endregion
    }
}

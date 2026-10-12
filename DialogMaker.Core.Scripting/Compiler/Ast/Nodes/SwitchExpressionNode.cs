using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents switch expression
    /// </summary>
    /// <param name="token">Token that represents switch keyword</param>
    public partial class SwitchExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Expression of comparing value
        /// </summary>
        public ExpressionNode? ValueExpression { get; set; }
        /// <summary>
        /// List of cases
        /// </summary>
        public List<CaseExpressionNode> Cases { get; set; } = [];

        #region Static

        /// <summary>
        /// Parse switch expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="valueExpression">Expression of comparing value</param>
        /// <returns>Parsed switch expression</returns>
        public static SwitchExpressionNode Parse(AstParserStream stream, ExpressionNode valueExpression)
        {
            var token = stream.Eat(DSharpTokenType.Switch);
            SwitchExpressionNode switchExpression = new(token)
            {
                ValueExpression = valueExpression
            };

            stream.Eat(DSharpTokenType.LeftBrace);

            while (!stream.Check(DSharpTokenType.RightBrace))
            {
                var caseStatement = CaseExpressionNode.Parse(stream);
                caseStatement.Parent = switchExpression;
                switchExpression.Cases.Add(caseStatement);

                if (!stream.Check(DSharpTokenType.Comma))
                {
                    break;
                }

                stream.Eat(DSharpTokenType.Comma);
            }

            stream.Eat(DSharpTokenType.RightBrace);

            valueExpression.Parent = switchExpression;

            return switchExpression;
        }

        #endregion
    }
}

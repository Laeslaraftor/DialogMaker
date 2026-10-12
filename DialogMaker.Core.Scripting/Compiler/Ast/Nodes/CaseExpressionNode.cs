using DialogMaker.Core.Scripting.Compiler.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents case expression
    /// </summary>
    /// <param name="token">Token that represents lambda</param>
    public partial class CaseExpressionNode(DSharpToken token) : ExpressionNode(token), ICaseNode
    {
        /// <summary>
        /// Expression of comparing value
        /// </summary>
        public ExpressionNode? KeyExpression { get; set; }
        /// <summary>
        /// Case value expression
        /// </summary>
        public ExpressionNode? ValueExpression { get; set; }
        /// <summary>
        /// Is default case
        /// </summary>
        public bool IsDefault { get; set; }
        AstNode? ICaseNode.ValueNode => ValueExpression;

        #region Controls

        AstNode ICaseNode.AsNode() => this;

        #endregion

        #region Static

        /// <summary>
        /// Parse case expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed case expression</returns>
        public static CaseExpressionNode Parse(AstParserStream stream)
        {
            var keyExpression = ParseExpression(stream);
            DSharpToken token;
            ExpressionNode valueExpression;

            if (keyExpression is DelegateExpressionNode delegateExpression &&
                delegateExpression.Parameters.Count == 1 &&
                delegateExpression.Body != null &&
                delegateExpression.Body.Statements.FirstOrDefault() is ExpressionStatementNode expressionBody &&
                expressionBody.Expression != null)
            {
                token = delegateExpression.Token;
                valueExpression = expressionBody.Expression;
                keyExpression = new IdentifierExpressionNode(delegateExpression.Parameters[0].Token);
            }
            else
            {
                token = stream.Eat(DSharpTokenType.Lambda);
                valueExpression = ParseExpression(stream);
            }

            CaseExpressionNode caseExpression = new(token)
            {
                KeyExpression = keyExpression,
                ValueExpression = valueExpression,
                IsDefault = keyExpression is IdentifierExpressionNode identifier && identifier.Name == "_"
            };

            keyExpression.Parent = caseExpression;
            valueExpression.Parent = caseExpression;

            return caseExpression;
        }

        #endregion
    }
}

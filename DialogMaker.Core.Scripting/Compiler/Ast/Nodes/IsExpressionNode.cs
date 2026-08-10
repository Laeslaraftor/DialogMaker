using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents <c>is</c> expression
    /// </summary>
    /// <param name="token">Token that represents <c>is</c> keyword</param>
    public abstract class IsExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Expression that checking by current <c>is</c> expression
        /// </summary>
        public ExpressionNode? CheckExpression { get; set; }
        /// <summary>
        /// Is current expression negative (is not)
        /// </summary>
        public bool IsNegative { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new(base.ToString());

            if (CheckExpression != null)
            {
                builder.AppendLine();
                builder.AppendLine($"Check expression: {CheckExpression}");
            }

            builder.Append($"Is negative: {IsNegative}");

            return builder.ToString();
        }

        #region Static

        /// <summary>
        /// Parse <c>is</c> expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="checkExpression">Expression that checking by current <c>is</c> expression</param>
        /// <returns>Parsed <c>is</c> expression</returns>
        public static IsExpressionNode Parse(AstParserStream stream, ExpressionNode checkExpression)
        {
            var token = stream.Eat(DSharpTokenType.Is);
            bool isNegative = false;

            if (stream.Check(DSharpTokenType.NotKeyword))
            {
                stream.Eat(DSharpTokenType.NotKeyword);
                isNegative = true;
            }
            if (LiteralExpressionNode.TryParse(stream, out var literalExpression))
            {
                return new IsLiteralValueExpressionNode(token)
                {
                    CheckExpression = checkExpression,
                    IsNegative = isNegative,
                    LiteralExpression = literalExpression
                };
            }

            IsTypeExpressionNode result = new(token)
            {
                CheckExpression = checkExpression,
                IsNegative = isNegative
            };

            if (TypeInfoNode.CanParse(stream, 0))
            {
                result.DestinationType = TypeInfoNode.Parse(stream, true, true);

                if (stream.Check(DSharpTokenType.Identifier))
                {
                    result.DestinationIdentifier = IdentifierExpressionNode.Parse(stream, false);
                }
            }
            if (stream.Check(DSharpTokenType.LeftBrace))
            {
                result.DestinationObjectValues = ObjectValuesNode.Parse(stream, DSharpTokenType.Colon);
            }

            return result;
        }

        #endregion
    }
}

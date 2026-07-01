using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    public class BinaryExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        public ExpressionNode? Left { get; set; }
        public DSharpBinaryOperator Operator { get; set; }
        public ExpressionNode? Right { get; set; }

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            if (Left == null)
            {
                return base.ToString();
            }

            StringBuilder builder = new();
            builder.AppendLine(base.ToString());
            builder.AppendLine($"Operator: {((DSharpTokenType)Operator)}");
            builder.AppendLine($"Left: {Left}");

            if (Right != null)
            {
                builder.Append($"Right: {Right}");
            }

            return builder.ToString();
        }

        #endregion

        #region Статика

        /// <summary>
        /// Check is current token binary operator
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser</param>
        /// <returns>Is current token binary operator</returns>
        public static bool IsBinaryOperator(AstParserStream stream)
        {
            var currentToken = stream.Current;

            if (currentToken == null)
            {
                return false;
            }

            var tokenType = currentToken.Type;

            return tokenType == DSharpTokenType.Plus ||
                   tokenType == DSharpTokenType.Minus ||
                   tokenType == DSharpTokenType.Multiply ||
                   tokenType == DSharpTokenType.Divide ||
                   tokenType == DSharpTokenType.Mod ||
                   tokenType == DSharpTokenType.Xor ||
                   tokenType == DSharpTokenType.ShiftLeft ||
                   tokenType == DSharpTokenType.ShiftRight ||
                   tokenType == DSharpTokenType.Or ||
                   tokenType == DSharpTokenType.And ||
                   tokenType == DSharpTokenType.Equal ||
                   tokenType == DSharpTokenType.NotEqual ||
                   tokenType == DSharpTokenType.Less ||
                   tokenType == DSharpTokenType.LessEqual ||
                   tokenType == DSharpTokenType.Greater ||
                   tokenType == DSharpTokenType.GreaterEqual;
        }

        public static ExpressionNode ParseLogicalOr(AstParserStream stream)
        {
            return ParseOperation(stream, ParseLogicalAnd, DSharpTokenType.Or);
        }
        public static ExpressionNode ParseLogicalAnd(AstParserStream stream)
        {
            return ParseOperation(stream, ParseEquality, DSharpTokenType.And);
        }
        public static ExpressionNode ParseEquality(AstParserStream stream)
        {
            return ParseOperation(stream, ParseComparison, DSharpTokenType.Equal,
                                                           DSharpTokenType.NotEqual);
        }
        public static ExpressionNode ParseComparison(AstParserStream stream)
        {
            return ParseOperation(stream, ParseAdditive, DSharpTokenType.Less,
                                                         DSharpTokenType.Greater,
                                                         DSharpTokenType.LessEqual,
                                                         DSharpTokenType.GreaterEqual);
        }
        public static ExpressionNode ParseAdditive(AstParserStream stream)
        {
            return ParseOperation(stream, ParseMultiplicative, DSharpTokenType.Plus,
                                                               DSharpTokenType.Minus);
        }
        public static ExpressionNode ParseMultiplicative(AstParserStream stream)
        {
            return ParseOperation(stream, ParseShift, DSharpTokenType.Multiply, 
                                                                     DSharpTokenType.Divide, 
                                                                     DSharpTokenType.Mod);
        }
        public static ExpressionNode ParseShift(AstParserStream stream)
        {
            return ParseOperation(stream, UnaryExpressionNode.Parse, DSharpTokenType.ShiftLeft, 
                                                                     DSharpTokenType.ShiftRight);
        }

        private static ExpressionNode ParseOperation(AstParserStream stream, Func<AstParserStream, ExpressionNode> parser, params DSharpTokenType[] tokens)
        {
            var left = parser(stream);

            while (stream.Check(tokens))
            {
                if (stream.Current == null)
                {
                    break;
                }

                var op = stream.Eat(stream.Current.Type);
                var right = parser(stream);
                left = new BinaryExpressionNode(op)
                {
                    Left = left,
                    Operator = (DSharpBinaryOperator)op.Type,
                    Right = right,
                };
            }

            return left;
        }

        #endregion
    }
}
